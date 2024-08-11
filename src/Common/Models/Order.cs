using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class Order
    {
        public string Id { get; set; }

        public string CustomerId { get; set; }

        public List<OrderItem> Items { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        
         public DateTime LastUpdated { get; set; }
    }

    public class OrderItem
    {
        public string ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}