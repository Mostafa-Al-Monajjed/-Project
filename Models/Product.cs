using System;

namespace StoreManagement.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastRestocked { get; internal set; }

        // public DateTime? LastRestocked { get; set; }
        // public bool IsActive { get; set; }

        public Product()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
           // IsActive = true;
            QuantityInStock = 0;
        }
    }
}
