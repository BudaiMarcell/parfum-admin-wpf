using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("is_admin")]
        public bool IsAdmin { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("zip_code")]
        public string ZipCode { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        public override string ToString() =>
            string.IsNullOrEmpty(City) ? Label ?? string.Empty : $"{ZipCode} {City}, {Street}";
    }
}
