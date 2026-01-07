using System;
using System.Collections.Generic;
using System.Linq;
using StoreManagement.Models;
using System.Text.RegularExpressions;
using StoreManagement.Interfaces;
using StoreManagement.Exceptions;

namespace StoreManagement.Services
{
    public class OrderService : ICartOperations
    {
        private readonly IFileRepository<Order> _repository;
        private readonly ProductService _productService;
        private List<OrderItem> _cartItems;
        private readonly UserService _userService;  


       public OrderService(IFileRepository<Order> repository, ProductService productService, UserService userService)
        {
            _repository = repository;
            _productService = productService;
            _userService = userService;  
            _cartItems = new List<OrderItem>();
        }

        public List<Order> GetAllOrders()
        {
            return _repository.LoadAll();
        }

        public Order GetOrderById(string id)
        {
            return _repository.GetById(id);
        }

        public List<Order> GetOrdersByCustomer(string customerName)
        {
            return GetAllOrders()
                .Where(o => o.CustomerName.Contains(customerName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Order> GetOrdersByDateRange(DateTime startDate, DateTime endDate)
        {
            return GetAllOrders()
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToList();
        }

        public List<Order> GetOrdersByMonth(int year, int month)
        {
            return GetAllOrders()
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                .ToList();
        }

        public decimal CalculateMonthlyProfit(int year, int month)
        {
            var orders = GetOrdersByMonth(year, month)
                .Where(o => o.Status == OrderStatus.Completed);
            
            return orders.Sum(o => o.TotalAmount);
        }

        
        public void AddItem(Product product, int quantity)
        {
            if (quantity <= 0)
                throw new InvalidInputException("Quantity must be positive");
            
            if (product.QuantityInStock < quantity)
                throw new InsufficientStockException($"Insufficient stock. Available: {product.QuantityInStock}");
            
            var existingItem = _cartItems.FirstOrDefault(i => i.ProductId == product.Id);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _cartItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });
            }
        }

        public void RemoveItem(string productId)
        {
            _cartItems.RemoveAll(i => i.ProductId == productId);
        }

        public void UpdateQuantity(string productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId);
                return;
            }
            
            var item = _cartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }

        public void ClearCart()
        {
            _cartItems.Clear();
        }

        public decimal CalculateTotal()
        {
            return _cartItems.Sum(i => i.TotalPrice);
        }

        public Order Checkout(string customerName, string phone, PaymentMethod paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new InvalidInputException("Customer name is required");
            
            if(!Regex.IsMatch(phone, @"^09\d{8}$"))
                throw new InvalidInputException("Invalid phone number format. Phone number must be in the format: 09xxxxxxxx (10 digits total, starting with 09)");
            
            if (_cartItems.Count == 0)
                throw new InvalidInputException("Cart is empty");
           
            var currentUser = _userService.CurrentUser;
            if (currentUser == null)
                throw new InvalidOperationException("No user is logged in");

            
            var order = new Order
            {
                CustomerName = customerName,
                CustomerPhone = phone,
                Items = new List<OrderItem>(_cartItems),
                TotalAmount = CalculateTotal(),
                Status = OrderStatus.Completed,
                EmployeeId = currentUser.Id,  
                PaymentMethod = paymentMethod
            };

            
            foreach (var item in _cartItems)
            {
                _productService.UpdateStock(item.ProductId, -item.Quantity);
            }

            
            _repository.Add(order);
            
            
            ClearCart();
            
            return order;
        }

        public List<OrderItem> GetCartItems()
        {
            return new List<OrderItem>(_cartItems);
        }

        public bool IsCartEmpty()
        {
            return _cartItems.Count == 0;
        }
    }
}
