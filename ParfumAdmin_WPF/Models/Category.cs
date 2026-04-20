using System.Text.Json.Serialization;

namespace ParfumAdmin_WPF.Models
{
    public class Category
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parent_id")]
        public int? ParentId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("children")]
        public List<Category> Children { get; set; }

        [JsonPropertyName("parent")]
        public Category Parent { get; set; }

        // A ComboBox (és más megjelenítések) alapból a ToString()-et hívják,
        // ha nincs template/DisplayMemberPath. Ezzel szülő-gyerek kategóriákat
        // egyértelműen meg lehet különböztetni (pl. "Férfi > Eau de Parfum").
        public override string ToString()
        {
            if (Parent != null && !string.IsNullOrEmpty(Parent.Name))
                return $"{Parent.Name} > {Name}";
            return Name ?? string.Empty;
        }
    }
}