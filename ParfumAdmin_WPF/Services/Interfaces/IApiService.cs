using ParfumAdmin_WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParfumAdmin_WPF.Services.Interfaces
{
    public interface IApiService
    {
        // Products
        Task<PaginatedResponse<Product>> GetProductsAsync(int page = 1, string search = null, string gender = null, int? categoryId = null, bool lowStock = false);
        Task<Product> GetProductAsync(int id);
        Task<Product> CreateProductAsync(object data);
        Task<Product> UpdateProductAsync(int id, object data);
        Task DeleteProductAsync(int id);
        Task BulkDeleteProductsAsync(IEnumerable<int> ids);
        Task BulkUpdateProductsAsync(IEnumerable<int> ids, object data);

        // Categories
        Task<List<Category>> GetAdminCategoriesAsync();
        Task<List<Category>> GetCategoriesAsync();

        // Orders
        Task<PaginatedResponse<Order>> GetOrdersAsync(int page = 1, string status = null, string search = null);
        Task<Order> GetOrderAsync(int id);
        Task<Order> UpdateOrderStatusAsync(int id, string status);
        Task<Order> UpdateOrderPaymentAsync(int id, string paymentStatus);
        Task DeleteOrderAsync(int id);

        // Coupons
        Task<PaginatedResponse<Coupon>> GetCouponsAsync(int page = 1, string search = null, string discountType = null, string status = null);
        Task<Coupon> GetCouponAsync(int id);
        Task<Coupon> CreateCouponAsync(object data);
        Task<Coupon> UpdateCouponAsync(int id, object data);
        Task DeleteCouponAsync(int id);

        // Audit logs
        Task<PaginatedResponse<AuditLog>> GetAuditLogsAsync(int page = 1, string action = null, string modelType = null, string search = null);

        // Analytics (raw — used by existing DashboardViewModel)
        Task<object> GetAnalyticsOverviewAsync();
        Task<object> GetAnalyticsHourlyAsync();
        Task<object> GetAnalyticsTopProductsAsync();
        Task<object> GetAnalyticsRealtimeAsync();

        // Analytics (typed — used by AnalyticsPage)
        Task<AnalyticsOverview> GetAnalyticsOverviewTypedAsync();
        Task<HourlySeries>      GetAnalyticsHourlyTypedAsync();
        Task<DailySeries>       GetAnalyticsDailyAsync(int days = 30);
        Task<DeviceStats>       GetAnalyticsDevicesAsync();
        Task<FunnelStats>       GetAnalyticsFunnelAsync();
    }
}
