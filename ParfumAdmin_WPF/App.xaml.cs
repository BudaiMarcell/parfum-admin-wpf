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
            services.AddSingleton<HttpClient>(provider =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
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

            // Views regisztrálása
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();

            // Pages regisztrálása
            services.AddTransient<DashboardPage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<OrdersPage>();

            ServiceProvider = services.BuildServiceProvider();

            // App indítása a LoginWindow-val
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }
}