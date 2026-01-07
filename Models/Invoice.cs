using System;
using System.Text;

namespace StoreManagement.Models
{
    public sealed class InvoiceBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private Order _order;
        private decimal _taxRate = 0.15m; 
        private const int ProductWidth = 20;
        private const int QtyWidth = 6;
        private const int PriceWidth = 12;
        private const int TotalWidth = 12;

        public InvoiceBuilder(Order order)
        {
            _order = order;
        }

        public InvoiceBuilder AddHeader()
        {
            _builder.AppendLine("=========================================");
            _builder.AppendLine("           Store MANAGEMENT SYSTEM   ");
            _builder.AppendLine("=========================================");
            _builder.AppendLine($"Invoice ID: {_order.Id}");
            _builder.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _builder.AppendLine($"Customer: {_order.CustomerName}");
            _builder.AppendLine($"Phone: {_order.CustomerPhone}");
            _builder.AppendLine("-----------------------------------------");
            return this;
        }


         public InvoiceBuilder AddItems()
        {
            _builder.AppendLine("Items:");
            _builder.AppendLine("-----------------------------------------");
            
            
            _builder.AppendLine(
                $"{"Product".PadRight(ProductWidth)}" +
                $"{"Qty".PadLeft(QtyWidth)}" +
                $"{"Price".PadLeft(PriceWidth)}" +
                $"{"Total".PadLeft(TotalWidth)}"
            );
            
            foreach (var item in _order.Items)
            {
                string productName = item.ProductName.Length > ProductWidth - 2 
                    ? item.ProductName.Substring(0, ProductWidth - 3) + "..." 
                    : item.ProductName;
                
                _builder.AppendLine(
                    $"{productName.PadRight(ProductWidth)}" +
                    $"{item.Quantity.ToString().PadLeft(QtyWidth)}" +
                    $"{item.UnitPrice.ToString("C").PadLeft(PriceWidth)}" +
                    $"{item.TotalPrice.ToString("C").PadLeft(TotalWidth)}"
                );
            }
            
            _builder.AppendLine("-----------------------------------------");
            return this;
        }

        
          public InvoiceBuilder AddTotals()
        {
            decimal subtotal = _order.TotalAmount;
            decimal tax = subtotal * _taxRate;
            decimal total = subtotal + tax;

            int totalsPadding = ProductWidth + QtyWidth + PriceWidth;
            
            _builder.AppendLine($"Subtotal:".PadRight(totalsPadding) + $"{subtotal:C}".PadLeft(TotalWidth));
            _builder.AppendLine($"Tax (15%):".PadRight(totalsPadding) + $"{tax:C}".PadLeft(TotalWidth));
            _builder.AppendLine($"Total:".PadRight(totalsPadding) + $"{total:C}".PadLeft(TotalWidth));
            
            _builder.AppendLine("-----------------------------------------");
            return this;
        }


        public InvoiceBuilder AddFooter()
        {
            _builder.AppendLine("Payment Method: " + _order.PaymentMethod);
            _builder.AppendLine("Status: " + _order.Status);
            _builder.AppendLine("Thank you for your business!");
            _builder.AppendLine("=========================================");
            return this;
        }

        public string Build()
        {
            return _builder.ToString();
        }
    }

    public class Invoice
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string Content { get; set; }
        public DateTime GeneratedAt { get; set; }

       
        public Invoice()
        {
            Id = string.Empty;
            OrderId = string.Empty;
            Content = string.Empty;
            GeneratedAt = DateTime.Now;
        }

       
        public Invoice(Order order)
        {
            Id = $"INV-{DateTime.Now:yyyyMMdd-HHmmss}";
            OrderId = order.Id;
            GeneratedAt = DateTime.Now;
            Content = GenerateInvoiceContent(order);
        }

        private string GenerateInvoiceContent(Order order)
        {
            var builder = new InvoiceBuilder(order);
            return builder
                .AddHeader()
                .AddItems()
                .AddTotals()
                .AddFooter()
                .Build();
        }
    }
}