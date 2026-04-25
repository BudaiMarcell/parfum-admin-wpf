using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class Product : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        private int _stockQuantity;
        [JsonPropertyName("stock_quantity")]
        public int StockQuantity
        {
            get => _stockQuantity;
            set { if (_stockQuantity != value) { _stockQuantity = value; OnPropertyChanged(); } }
        }

        [JsonPropertyName("volume_ml")]
        public int? VolumeMl { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        private bool _isActive;
        [JsonPropertyName("is_active")]
        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(); } }
        }

        [JsonPropertyName("category")]
        public Category Category { get; set; }

        [JsonPropertyName("primary_image")]
        public ProductImage PrimaryImage { get; set; }

        // Null, ha a termék él. Ha a Laravel `withTrashed()`-del küldi vissza,
        // ez a mező kitöltve jön és jelzi, hogy a termék soft-delete-elt.
        [JsonPropertyName("deleted_at")]
        public System.DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public bool IsDeleted => DeletedAt.HasValue;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ProductImage
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("is_primary")]
        public bool IsPrimary { get; set; }

        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }
    }
}