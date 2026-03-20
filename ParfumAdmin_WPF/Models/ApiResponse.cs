using System;
using System.Collections.Generic;
using System.Text;

namespace ParfumAdmin_WPF.Models
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public int LastPage { get; set; }
        public int Total { get; set; }
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
