using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.Views.Pages;

namespace ParfumAdmin_WPF.Views
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;

        public MainWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            // Induláskor a Dashboard oldalt mutatjuk
            NavigateTo("Dashboard");
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                NavigateTo(tag);
            }
        }

        private void NavigateTo(string page)
        {
            switch (page)
            {
                case "Dashboard":
                    MainFrame.Navigate(App.ServiceProvider.GetRequiredService<DashboardPage>());
                    break;
                case "Products":
                    MainFrame.Navigate(App.ServiceProvider.GetRequiredService<ProductsPage>());
                    break;
                case "Orders":
                    MainFrame.Navigate(App.ServiceProvider.GetRequiredService<OrdersPage>());
                    break;
                case "Coupons":
                    MainFrame.Navigate(App.ServiceProvider.GetRequiredService<CouponsPage>());
                    break;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfirmDialog("Kijelentkezés", "Biztos ki szeretnél jelentkezni?")
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            _authService.Logout();

            var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }
    }
}