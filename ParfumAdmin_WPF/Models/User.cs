using System;
using System.Collections.Generic;
using System.Text;

namespace ParfumAdmin_WPF.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string FullAddress { get; set; }
        public bool IsDefault { get; set; }
    }
}
