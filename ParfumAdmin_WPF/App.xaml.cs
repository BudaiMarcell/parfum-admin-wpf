using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels;
using ParfumAdmin_WPF.Views;
using ParfumAdmin_WPF.Views.Pages;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace ParfumAdmin_WPF
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ── Configuration ─────────────────────────────────────────────
            var basePath = AppContext.BaseDirectory;
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // Fail-fast if BaseUrl is missing or unparseable so the user
            // gets a clear error during startup instead of silently broken HTTP.
            var apiSection = config.GetSection("Api");
            var baseUrl = apiSection["BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl) ||
                !Uri.TryCreate(baseUrl, UriKind.Absolute, out var apiUri))
            {
                MessageBox.Show(
                    "Hiányzik vagy érvénytelen az Api:BaseUrl beállítás az appsettings.json fájlban.\n\n" +
                    $"Várt elérési út: {Path.Combine(basePath, "appsettings.json")}",
                    "Konfigurációs hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            // ── Logging ───────────────────────────────────────────────────
            // Rolling daily file with 7-day retention. The path uses a
            // .NET style placeholder; Serilog rolls "app-.log" → "app-20260425.log".
            // Tokens, email bodies and request payloads are NEVER logged.
            var logPathTemplate = config["Logging:File:Path"]
                ?? "%LOCALAPPDATA%\\ParfumAdmin\\logs\\app-.log";
            var logPath = Environment.ExpandEnvironmentVariables(logPathTemplate);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            }
            catch
            {
                // Logging is best-effort; if we can't create the directory
                // we let Serilog fail silently rather than blocking startup.
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("ParfumAdmin starting (api={ApiUri})", apiUri);

            // ── DI container ──────────────────────────────────────────────
            var services = new ServiceCollection();

            services.AddSingleton(config);
            services.Configure<ApiOptions>(apiSection);

            // Auth/session state singletons.
            services.AddSingleton<TokenStore>();
            services.AddSingleton<IAuthState, AuthState>();

            // The handler must be registered as a service so AddHttpClient
            // can resolve it via .AddHttpMessageHandler<T>().
            services.AddTransient<AuthDelegatingHandler>();

            // ── Typed HttpClients ─────────────────────────────────────────
            // Two named/typed clients with separate header lifecycles:
            //
            //  • IAuthService — used only for /login. No DelegatingHandler so
            //    a stale token doesn't get attached to login attempts and
            //    a 401 (= wrong password) doesn't fire SessionExpired.
            //
            //  • IApiService  — admin endpoints. AuthDelegatingHandler reads
            //    the token from TokenStore and attaches Authorization on every
            //    outgoing request, plus intercepts 401 to clear the store.
            //
            // Both inherit BaseAddress from the same options binding so the
            // BaseUrl never appears in service code.
            services.AddHttpClient<IAuthService, AuthService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
                                       | System.Net.DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                ConnectTimeout = TimeSpan.FromSeconds(5),
            });

            services.AddHttpClient<IApiService, ApiService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
                                       | System.Net.DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                ConnectTimeout = TimeSpan.FromSeconds(5),
            })
            .AddHttpMessageHandler<AuthDelegatingHandler>();

            // ── ViewModels ────────────────────────────────────────────────
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<OrdersViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ProductFormViewModel>();
            services.AddTransient<CouponsViewModel>();
            services.AddTransient<CouponFormViewModel>();
            services.AddTransient<AuditLogsViewModel>();
            services.AddTransient<AnalyticsViewModel>();

            // ── Views ─────────────────────────────────────────────────────
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<ProductFormWindow>();
            services.AddTransient<CouponFormWindow>();

            // ── Pages ─────────────────────────────────────────────────────
            services.AddTransient<DashboardPage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<OrdersPage>();
            services.AddTransient<CouponsPage>();
            services.AddTransient<AuditLogsPage>();
            services.AddTransient<AnalyticsPage>();

            ServiceProvider = services.BuildServiceProvider();

            // ── Startup window ────────────────────────────────────────────
            // If we have a persisted token, optimistically open the
            // dashboard. The first ApiService request will go out with the
            // stored token; if the server rejects it (401), the
            // AuthDelegatingHandler clears the store and the SessionExpired
            // event bounces the user back to the login screen.
            var tokenStore = ServiceProvider.GetRequiredService<TokenStore>();
            var hasToken = !string.IsNullOrEmpty(tokenStore.Load());

            Window initialWindow = hasToken
                ? ServiceProvider.GetRequiredService<MainWindow>()
                : ServiceProvider.GetRequiredService<LoginWindow>();
            initialWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
