using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                // A Laravel a decimal mezőket stringként küldi ("price":"89000.00"),
                // ez engedélyezi, hogy számként olvassuk be.
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
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
        public async Task<PaginatedResponse<Product>> GetProductsAsync(int page = 1, string search = null, string gender = null, int? categoryId = null, bool lowStock = false)
        {
            var url = $"/products?page={page}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrEmpty(gender))
                url += $"&gender={gender}";
            if (categoryId.HasValue && categoryId.Value > 0)
                url += $"&category_id={categoryId.Value}";
            if (lowStock)
                url += "&low_stock=1";
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

        public async Task BulkDeleteProductsAsync(IEnumerable<int> ids) =>
            await PostAsync<object>("/products/bulk-delete", new { ids });

        public async Task BulkUpdateProductsAsync(IEnumerable<int> ids, object data)
        {
            // összefésüljük az ids-t és a többi mezőt egy payloadba
            var json = JsonSerializer.Serialize(data);
            using var doc = JsonDocument.Parse(json);
            var dict = new Dictionary<string, object> { ["ids"] = ids };
            foreach (var p in doc.RootElement.EnumerateObject())
                dict[p.Name] = JsonSerializer.Deserialize<object>(p.Value.GetRawText());
            await PostAsync<object>("/products/bulk-update", dict);
        }

        // Orders
        public async Task<PaginatedResponse<Order>> GetOrdersAsync(int page = 1, string status = null, string search = null)
        {
            var url = $"/orders?page={page}";
            if (!string.IsNullOrEmpty(status))
                url += $"&status={status}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            return await GetAsync<PaginatedResponse<Order>>(url);
        }

        public async Task<Order> GetOrderAsync(int id) =>
            await GetAsync<Order>($"/orders/{id}");

        public async Task<Order> UpdateOrderStatusAsync(int id, string status) =>
            await PutAsync<Order>($"/orders/{id}/status", new { status });

        public async Task<Order> UpdateOrderPaymentAsync(int id, string paymentStatus) =>
            await PutAsync<Order>($"/orders/{id}/payment", new { payment_status = paymentStatus });

        public async Task DeleteOrderAsync(int id) =>
            await DeleteAsync($"/orders/{id}");

        // Coupons
        public async Task<PaginatedResponse<Coupon>> GetCouponsAsync(int page = 1, string search = null, string discountType = null, string status = null)
        {
            var url = $"/coupons?page={page}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrEmpty(discountType))
                url += $"&discount_type={discountType}";
            if (!string.IsNullOrEmpty(status))
                url += $"&status={status}";
            return await GetAsync<PaginatedResponse<Coupon>>(url);
        }

        public async Task<Coupon> GetCouponAsync(int id) =>
            await GetAsync<Coupon>($"/coupons/{id}");

        public async Task<Coupon> CreateCouponAsync(object data) =>
            await PostAsync<Coupon>("/coupons", data);

        public async Task<Coupon> UpdateCouponAsync(int id, object data) =>
            await PutAsync<Coupon>($"/coupons/{id}", data);

        public async Task DeleteCouponAsync(int id) =>
            await DeleteAsync($"/coupons/{id}");

        // Categories
        public async Task<List<Category>> GetAdminCategoriesAsync() =>
            await GetAsync<List<Category>>("/categories");

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
