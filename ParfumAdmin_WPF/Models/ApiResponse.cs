using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
    }

    public class ApiListResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }

    public class PaginatedResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }

        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }
    }

    public class LoginResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }

    public class TopProduct
    {
        public int ProductId { get; set; }
        public int ViewCount { get; set; }
        public Product Product { get; set; }
    }
}
