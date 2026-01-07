using System;
using System.Collections.Generic;
using System.Linq;
using StoreManagement.Models;
using StoreManagement.Interfaces;

namespace StoreManagement.Services
{
    public class InvoiceService
    {
        private readonly IFileRepository<Invoice> _repository;
        private readonly OrderService _orderService;

        public InvoiceService(IFileRepository<Invoice> repository, OrderService orderService)
        {
            _repository = repository;
            _orderService = orderService;
        }

        public Invoice GenerateInvoice(Order order)
        {
            var invoice = new Invoice(order);
            _repository.Add(invoice);
            return invoice;
        }

        public List<Invoice> GetInvoicesByCustomer(string customerName)
        {
            var orders = _orderService.GetOrdersByCustomer(customerName);
            return orders.Select(o => new Invoice(o)).ToList();
        }

        public List<Invoice> GetMonthlyInvoices(int year, int month)
        {
            var orders = _orderService.GetOrdersByMonth(year, month);
            return orders.Select(o => new Invoice(o)).ToList();
        }

        public string GenerateMonthlyReport(int year, int month)
        {
            var orders = _orderService.GetOrdersByMonth(year, month)
                .Where(o => o.Status == OrderStatus.Completed)
                .ToList();
            
            var totalProfit = _orderService.CalculateMonthlyProfit(year, month);
            var totalOrders = orders.Count;
            var totalItems = orders.Sum(o => o.Items.Sum(i => i.Quantity));

            var report = new System.Text.StringBuilder();
            report.AppendLine("=========================================");
            report.AppendLine($"      MONTHLY SALES REPORT - {month}/{year}");
            report.AppendLine("=========================================");
            report.AppendLine($"Total Orders: {totalOrders}");
            report.AppendLine($"Total Items Sold: {totalItems}");
            report.AppendLine($"Total Revenue: {totalProfit:C}");
            report.AppendLine("-----------------------------------------");
            report.AppendLine("Top Selling Products:");
            
            var productSales = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductName)
                .Select(g => new { Product = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(5);
            
            foreach (var sale in productSales)
            {
                report.AppendLine($"  {sale.Product}: {sale.Quantity} units");
            }
            
            report.AppendLine("=========================================");
            
            return report.ToString();
        }
    }
}
