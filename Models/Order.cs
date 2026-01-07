using System;
using System.Collections.Generic;

namespace StoreManagement.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Completed,
        Cancelled
    }

   public class Order
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string EmployeeId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public Order()
        {
            Id = $"ORD-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 4)}";
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            Items = new List<OrderItem>();
            TotalAmount = 0;
            OrderDate = DateTime.Now;
            Status = OrderStatus.Pending;
            EmployeeId = string.Empty;  
            PaymentMethod = PaymentMethod.Cash;
        }
    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard
    }
}
