using ParfumAdmin_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParfumAdmin_WPF.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        void Logout();
        bool IsLoggedIn();
        string GetToken();
    }
}
