using Microsoft.Extensions.DependencyInjection;
using ParfumAdmin_WPF;
using ParfumAdmin_WPF.Services;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels;
using ParfumAdmin_WPF.Views;
using ParfumAdmin_WPF.Views.Pages;
using System.Net.Http;
using System.Windows;

namespace ParfumAdmin_WPF
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // HttpClient regisztrálása
            // - AutomaticDecompression: gzip/deflate csökkenti a hálózati átvitelt (Laravel küld gzip-et, ha a kliens Accept-Encoding-al jelzi).
            // - Timeout: 15s, hogy egy leállt szerver ne fagyassza be az UI-t korlátlan ideig.
            // - ConnectionClose=false + PooledConnectionLifetime: tartja a TCP kapcsolatot, így csak az első kérés lassú.
            services.AddSingleton<HttpClient>(provider =>
            {
                var handler = new SocketsHttpHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip
                                           | System.Net.DecompressionMethods.Deflate,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                    ConnectTimeout = TimeSpan.FromSeconds(5),
                };

                var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(15),
                };
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
                return client;
            });

            // Services regisztrálása
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IApiService, ApiService>();

            // ViewModels regisztrálása
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<OrdersViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ProductFormViewModel>();
            services.AddTransient<CouponsViewModel>();
            services.AddTransient<CouponFormViewModel>();
            services.AddTransient<AuditLogsViewModel>();
            services.AddTransient<AnalyticsViewModel>();

            // Views regisztrálása
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<ProductFormWindow>();
            services.AddTransient<CouponFormWindow>();

            // Pages regisztrálása
            services.AddTransient<DashboardPage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<OrdersPage>();
            services.AddTransient<CouponsPage>();
            services.AddTransient<AuditLogsPage>();
            services.AddTransient<AnalyticsPage>();

            ServiceProvider = services.BuildServiceProvider();

            // App indítása a LoginWindow-val
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }
}