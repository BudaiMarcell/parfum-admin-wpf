using System;
using System.Collections.Generic;
using System.Text;

namespace ParfumAdmin_WPF.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string VolumeML { get; set; }
        public string Gender { get; set; }
        public bool IsActive { get; set; }
        public bool InStock { get; set; }
        public string CreatedAt { get; set; }
        public Category Category { get; set; }
        public ProductImage PrimaryImage { get; set; }
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }
}
