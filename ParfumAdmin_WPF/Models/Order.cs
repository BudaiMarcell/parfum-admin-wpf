using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class Order : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("user_id")]
        public int? UserId { get; set; }

        [JsonPropertyName("address_id")]
        public int? AddressId { get; set; }

        private string _status;
        [JsonPropertyName("status")]
        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; }

        private string _paymentStatus;
        [JsonPropertyName("payment_status")]
        public string PaymentStatus
        {
            get => _paymentStatus;
            set { if (_paymentStatus != value) { _paymentStatus = value; OnPropertyChanged(); } }
        }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        /// <summary>
        /// Az items_count értékét a Laravel withCount('items') adja.
        /// </summary>
        [JsonPropertyName("items_count")]
        public int ItemsCount { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class OrderItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("product")]
        public Product Product { get; set; }
    }
}
