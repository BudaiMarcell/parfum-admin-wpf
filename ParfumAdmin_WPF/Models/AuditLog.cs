using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class AuditLog
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("user_id")]
        public int? UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("model_type")]
        public string ModelType { get; set; }

        [JsonPropertyName("model_id")]
        public int? ModelId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        // A részletes diff JSON-t szövegként is tartjuk, hogy megjeleníthessük.
        [JsonPropertyName("changes")]
        public object Changes { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }
}
