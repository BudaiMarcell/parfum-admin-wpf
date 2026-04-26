using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
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

        public ApiService(HttpClient httpClient)
        {
            // BaseAddress is set in App.OnStartup from appsettings.json. Every
            // endpoint URL below is therefore relative ("admin/products"); a
            // leading slash would reset to the host root and skip /api.
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                // A Laravel a decimal mezőket stringként küldi ("price":"89000.00"),
                // ez engedélyezi, hogy számként olvassuk be.
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }

        // Hungarian message displayed to the user when the server returns a
        // non-2xx. We deliberately do NOT include the status body — that would
        // leak validation details and stack traces to the admin UI.
        private static ApiException FromResponse(HttpResponseMessage response) =>
            new ApiException(response.StatusCode,
                $"A szerver hibát adott vissza ({(int)response.StatusCode}). Próbáld újra később.");

        private async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw FromResponse(response);

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        private async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw FromResponse(response);

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        private async Task<T> PutAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw FromResponse(response);

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        private async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);

            if (!response.IsSuccessStatusCode)
                throw FromResponse(response);
        }

        // Products
        public async Task<PaginatedResponse<Product>> GetProductsAsync(int page = 1, string search = null, string gender = null, int? categoryId = null, bool lowStock = false)
        {
            var url = $"admin/products?page={page}";
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
            await GetAsync<Product>($"admin/products/{id}");

        public async Task<Product> CreateProductAsync(object data) =>
            await PostAsync<Product>("admin/products", data);

        public async Task<Product> UpdateProductAsync(int id, object data) =>
            await PutAsync<Product>($"admin/products/{id}", data);

        public async Task DeleteProductAsync(int id) =>
            await DeleteAsync($"admin/products/{id}");

        public async Task BulkDeleteProductsAsync(IEnumerable<int> ids) =>
            await PostAsync<object>("admin/products/bulk-delete", new { ids });

        public async Task BulkUpdateProductsAsync(IEnumerable<int> ids, object data)
        {
            // összefésüljük az ids-t és a többi mezőt egy payloadba
            var json = JsonSerializer.Serialize(data);
            using var doc = JsonDocument.Parse(json);
            var dict = new Dictionary<string, object> { ["ids"] = ids };
            foreach (var p in doc.RootElement.EnumerateObject())
                dict[p.Name] = JsonSerializer.Deserialize<object>(p.Value.GetRawText());
            await PostAsync<object>("admin/products/bulk-update", dict);
        }

        // Orders
        public async Task<PaginatedResponse<Order>> GetOrdersAsync(int page = 1, string status = null, string search = null)
        {
            var url = $"admin/orders?page={page}";
            if (!string.IsNullOrEmpty(status))
                url += $"&status={status}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            return await GetAsync<PaginatedResponse<Order>>(url);
        }

        public async Task<Order> GetOrderAsync(int id) =>
            await GetAsync<Order>($"admin/orders/{id}");

        public async Task<Order> UpdateOrderStatusAsync(int id, string status) =>
            await PutAsync<Order>($"admin/orders/{id}/status", new { status });

        public async Task<Order> UpdateOrderPaymentAsync(int id, string paymentStatus) =>
            await PutAsync<Order>($"admin/orders/{id}/payment", new { payment_status = paymentStatus });

        public async Task DeleteOrderAsync(int id) =>
            await DeleteAsync($"admin/orders/{id}");

        // Coupons
        public async Task<PaginatedResponse<Coupon>> GetCouponsAsync(int page = 1, string search = null, string discountType = null, string status = null)
        {
            var url = $"admin/coupons?page={page}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrEmpty(discountType))
                url += $"&discount_type={discountType}";
            if (!string.IsNullOrEmpty(status))
                url += $"&status={status}";
            return await GetAsync<PaginatedResponse<Coupon>>(url);
        }

        public async Task<Coupon> GetCouponAsync(int id) =>
            await GetAsync<Coupon>($"admin/coupons/{id}");

        public async Task<Coupon> CreateCouponAsync(object data) =>
            await PostAsync<Coupon>("admin/coupons", data);

        public async Task<Coupon> UpdateCouponAsync(int id, object data) =>
            await PutAsync<Coupon>($"admin/coupons/{id}", data);

        public async Task DeleteCouponAsync(int id) =>
            await DeleteAsync($"admin/coupons/{id}");

        // Categories
        public async Task<List<Category>> GetAdminCategoriesAsync() =>
            await GetAsync<List<Category>>("admin/categories");

        public async Task<List<Category>> GetCategoriesAsync() =>
            await GetAsync<List<Category>>("admin/categories");

        // Audit logs
        public async Task<PaginatedResponse<AuditLog>> GetAuditLogsAsync(int page = 1, string action = null, string modelType = null, string search = null)
        {
            var url = $"admin/audit-logs?page={page}";
            if (!string.IsNullOrEmpty(action))    url += $"&action={action}";
            if (!string.IsNullOrEmpty(modelType)) url += $"&model_type={modelType}";
            if (!string.IsNullOrEmpty(search))    url += $"&search={Uri.EscapeDataString(search)}";
            return await GetAsync<PaginatedResponse<AuditLog>>(url);
        }

        // Analytics (raw)
        public async Task<object> GetAnalyticsOverviewAsync() =>
            await GetAsync<object>("admin/analytics/overview");

        public async Task<object> GetAnalyticsHourlyAsync() =>
            await GetAsync<object>("admin/analytics/hourly");

        public async Task<object> GetAnalyticsTopProductsAsync() =>
            await GetAsync<object>("admin/analytics/top-products");

        public async Task<object> GetAnalyticsRealtimeAsync() =>
            await GetAsync<object>("admin/analytics/realtime");

        // Analytics (typed)
        public async Task<AnalyticsOverview> GetAnalyticsOverviewTypedAsync() =>
            await GetAsync<AnalyticsOverview>("admin/analytics/overview");

        public async Task<HourlySeries> GetAnalyticsHourlyTypedAsync() =>
            await GetAsync<HourlySeries>("admin/analytics/hourly");

        public async Task<DailySeries> GetAnalyticsDailyAsync(int days = 30) =>
            await GetAsync<DailySeries>($"admin/analytics/daily?days={days}");

        public async Task<DeviceStats> GetAnalyticsDevicesAsync() =>
            await GetAsync<DeviceStats>("admin/analytics/devices");

        public async Task<FunnelStats> GetAnalyticsFunnelAsync() =>
            await GetAsync<FunnelStats>("admin/analytics/funnel");
    }
}
