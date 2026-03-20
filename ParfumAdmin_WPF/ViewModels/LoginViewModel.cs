using ParfumAdmin_WPF.Helpers;
using ParfumAdmin_WPF.Services.Interfaces;
using ParfumAdmin_WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ParfumAdmin_WPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _errorMessage;
        public new string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public event Action OnLoginSuccess;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async _ => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "Add meg az email címed.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                await _authService.LoginAsync(Email, Password);
                OnLoginSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // A jelszót nem tároljuk ViewModel-ben – a View adja át közvetlenül
        public string Password { get; set; }
    }
}
