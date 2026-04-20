using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ParfumAdmin_WPF.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private string _token;
        private const string BaseUrl = "http://localhost:8000/api";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            // Stale Authorization header-t töröljük a login előtt,
            // hogy egy korábbi token ne zavarja a bejelentkezést.
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var payload = new { email, password };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;
            try
            {
                response = await _httpClient.PostAsync($"{BaseUrl}/login", content);
                body = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(
                    "Nem sikerült elérni a szervert (" + BaseUrl + "). " +
                    "Fut a Laravel? Részletek: " + ex.Message);
            }

            System.Diagnostics.Debug.WriteLine($"LOGIN STATUS: {(int)response.StatusCode} {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"LOGIN BODY:   {body}");

            if (!response.IsSuccessStatusCode)
            {
                // 401 = rossz jelszó, 422 = validációs hiba, egyéb = szerverhiba
                var shortBody = body.Length > 300 ? body.Substring(0, 300) + "..." : body;
                throw new Exception($"Bejelentkezés sikertelen ({(int)response.StatusCode}): {shortBody}");
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<LoginResponse>(body, options);

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                throw new Exception("A szerver válasza érvénytelen (hiányzó token).");
            }

            _token = result.Token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            return result;
        }

        public void Logout()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public bool IsLoggedIn() => !string.IsNullOrEmpty(_token);

        public string GetToken() => _token;
    }
}
