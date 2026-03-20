using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ParfumAdmin_WPF.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string BaseUrl = "http://localhost:8000/api/admin";

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}{endpoint}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hiba: {response.StatusCode}");

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        private async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}{endpoint}", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hiba: {response.StatusCode}");

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        private async Task<T> PutAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}{endpoint}", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hiba: {response.StatusCode}");

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        private async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}{endpoint}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hiba: {response.StatusCode}");
        }

        // Products
        public async Task<PaginatedResponse<Product>> GetProductsAsync(int page = 1, string search = null)
        {
            var url = $"/products?page={page}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={search}";
            return await GetAsync<PaginatedResponse<Product>>(url);
        }

        public async Task<Product> GetProductAsync(int id) =>
            await GetAsync<Product>($"/products/{id}");

        public async Task<Product> CreateProductAsync(object data) =>
            await PostAsync<Product>("/products", data);

        public async Task<Product> UpdateProductAsync(int id, object data) =>
            await PutAsync<Product>($"/products/{id}", data);

        public async Task DeleteProductAsync(int id) =>
            await DeleteAsync($"/products/{id}");

        // Orders
        public async Task<PaginatedResponse<Order>> GetOrdersAsync(int page = 1, string status = null)
        {
            var url = $"/orders?page={page}";
            if (!string.IsNullOrEmpty(status))
                url += $"&status={status}";
            return await GetAsync<PaginatedResponse<Order>>(url);
        }

        public async Task<Order> GetOrderAsync(int id) =>
            await GetAsync<Order>($"/orders/{id}");

        public async Task<Order> UpdateOrderStatusAsync(int id, string status) =>
            await PutAsync<Order>($"/orders/{id}/status", new { status });

        public async Task<Order> UpdateOrderPaymentAsync(int id, string paymentStatus) =>
            await PutAsync<Order>($"/orders/{id}/payment", new { payment_status = paymentStatus });

        // Categories
        public async Task<List<Category>> GetCategoriesAsync() =>
            await GetAsync<List<Category>>("/categories");

        // Analytics
        public async Task<object> GetAnalyticsOverviewAsync() =>
            await GetAsync<object>("/analytics/overview");

        public async Task<object> GetAnalyticsHourlyAsync() =>
            await GetAsync<object>("/analytics/hourly");

        public async Task<object> GetAnalyticsTopProductsAsync() =>
            await GetAsync<object>("/analytics/top-products");

        public async Task<object> GetAnalyticsRealtimeAsync() =>
            await GetAsync<object>("/analytics/realtime");
    }
}
