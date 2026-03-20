using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ParfumAdmin_WPF.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string Notes { get; set; }
        public string CreatedAt { get; set; }
        public int ItemsCount { get; set; }
        public Address Address { get; set; }
        public User User { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public Product Product { get; set; }
    }
}
