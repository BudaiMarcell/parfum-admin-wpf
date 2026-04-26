using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ParfumAdmin_WPF.Services;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.Views.Pages;

namespace ParfumAdmin_WPF.Views
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IAuthState _authState;

        public MainWindow(IAuthService authService, IAuthState authState)
        {
            InitializeComponent();
            _authService = authService;
            _authState = authState;

            // Bounce back to the login view when the session ends — either
            // because the user clicked Logout or because the server returned
            // 401 (token expired/revoked) on a background request.
            _authState.SessionExpired += OnSessionExpired;
            Closed += (_, _) => _authState.SessionExpired -= OnSessionExpired;

            // Induláskor a Dashboard oldalt mutatjuk
            NavigateTo("Dashboard");
        }

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            // The handler can fire from a background HTTP thread. Marshal
            // back to the UI thread before touching window state.
            Dispatcher.Invoke(() =>
            {
                if (!IsLoaded) return;

                var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                Close();
            });
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
                case "AuditLogs":
                    MainFrame.Navigate(App.ServiceProvider.GetRequiredService<AuditLogsPage>());
                    break;
                case "Analytics":
                    MainFrame.Navigate(App.ServiceProvider.GetRequiredService<AnalyticsPage>());
                    break;
            }
        }

        /// <summary>Called by DashboardPage when a stat box is clicked.</summary>
        public void NavigateToPage(string page) => NavigateTo(page);

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfirmDialog("Kijelentkezés", "Biztos ki szeretnél jelentkezni?")
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
                return;

            // Logout() raises SessionExpired; OnSessionExpired handles the
            // window swap so we don't duplicate that logic here.
            _authService.Logout();
        }
    }
}
