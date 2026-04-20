using Microsoft.Extensions.DependencyInjection;
using ParfumAdmin_WPF.ViewModels;
using System.Windows;

namespace ParfumAdmin_WPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnLoginSuccess += OpenMainWindow;

            PasswordBox.PasswordChanged += (s, e) =>
            {
                _viewModel.Password = PasswordBox.Password;
            };
        }

        private void OpenMainWindow()
        {
            var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.OnLoginSuccess -= OpenMainWindow;
            base.OnClosing(e);
        }
    }
}