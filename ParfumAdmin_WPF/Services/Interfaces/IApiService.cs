using ParfumAdmin_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParfumAdmin_WPF.Services.Interfaces
{
    public interface IApiService
    {
        Task<PaginatedResponse<Product>> GetProductsAsync(int page = 1, string search = null);
        Task<Product> GetProductAsync(int id);
        Task<Product> CreateProductAsync(object data);
        Task<Product> UpdateProductAsync(int id, object data);
        Task DeleteProductAsync(int id);

        Task<PaginatedResponse<Order>> GetOrdersAsync(int page = 1, string status = null);
        Task<Order> GetOrderAsync(int id);
        Task<Order> UpdateOrderStatusAsync(int id, string status);
        Task<Order> UpdateOrderPaymentAsync(int id, string paymentStatus);

        Task<List<Category>> GetCategoriesAsync();

        Task<object> GetAnalyticsOverviewAsync();
        Task<object> GetAnalyticsHourlyAsync();
        Task<object> GetAnalyticsTopProductsAsync();

        Task<object> GetAnalyticsRealtimeAsync();
    }
}
