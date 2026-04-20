using System;
using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class Coupon
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("coupon_code")]
        public string CouponCode { get; set; } = string.Empty;

        [JsonPropertyName("discount_type")]
        public string DiscountType { get; set; } = "percentage";

        [JsonPropertyName("discount_value")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [JsonPropertyName("usage_limit")]
        public int? UsageLimit { get; set; }

        [JsonPropertyName("used_count")]
        public int UsedCount { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // UI-csak mezők (nem küldjük a szervernek)
        [JsonIgnore]
        public string FormattedValue =>
            DiscountType == "percentage"
                ? $"{DiscountValue:0.##}%"
                : $"{DiscountValue:N0} Ft";

        [JsonIgnore]
        public string UsageText =>
            UsageLimit.HasValue
                ? $"{UsedCount} / {UsageLimit}"
                : $"{UsedCount} / ∞";

        [JsonIgnore]
        public bool IsExpired => ExpiryDate.Date < DateTime.Today;

        [JsonIgnore]
        public string StatusText =>
            IsExpired ? "Lejárt"
            : !IsActive ? "Inaktív"
            : (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) ? "Elfogyott"
            : "Aktív";
    }
}
