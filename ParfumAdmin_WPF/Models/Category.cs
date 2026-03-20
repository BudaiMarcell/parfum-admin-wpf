using System;
using System.Collections.Generic;
using System.Text;

namespace ParfumAdmin_WPF.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public List<Category> Children { get; set; }
    }
}
