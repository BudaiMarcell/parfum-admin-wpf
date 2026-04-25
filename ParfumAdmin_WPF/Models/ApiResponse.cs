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
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        // Összesen eladott darabszám (SUM(order_items.quantity))
        [JsonPropertyName("total_qty")]
        public int SoldQty { get; set; }

        // Összesen hozott bevétel (SUM(order_items.subtotal))
        [JsonPropertyName("total_revenue")]
        public decimal TotalRevenue { get; set; }

        [JsonPropertyName("product")]
        public Product Product { get; set; }

        // Kijelzésre szánt név. Ha a termék azóta törlődött (soft-delete),
        // "(törölve)" utótagot tesz a név végére. Ha a product egyáltalán
        // null jön vissza (pl. hard-delete-elt sor), egy placeholdert ír.
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (Product == null) return $"#{ProductId} (eltávolítva)";
                return Product.IsDeleted
                    ? $"{Product.Name} (törölve)"
                    : Product.Name;
            }
        }

        [JsonIgnore]
        public bool IsDeletedProduct => Product == null || Product.IsDeleted;
    }
}
