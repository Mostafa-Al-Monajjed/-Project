using System.Collections.Generic;
using System.Linq;
using StoreManagement.Models;
using StoreManagement.Interfaces;
using StoreManagement.Exceptions;

namespace StoreManagement.Services
{
    public class ProductService
    {
        private readonly IFileRepository<Product> _repository;
        private readonly CategoryService _categoryService;

        public ProductService(IFileRepository<Product> repository, CategoryService categoryService)
        {
            _repository = repository;
            _categoryService = categoryService;
        }

        public List<Product> GetAllProducts()
        {
            return _repository.LoadAll();
        }

        public Product GetProductById(string id)
        {
            var product = _repository.GetById(id);
            if (product == null)
                throw new InvalidProductException($"Product with ID {id} not found");
            return product;
        }

        public List<Product> GetProductsByCategory(string categoryId)
        {
            return GetAllProducts().Where(p => p.CategoryId == categoryId).ToList();
        }

        public void AddProduct(Product product)
        {
            ValidateProduct(product);
            
            
            try
            {
                _categoryService.GetCategoryById(product.CategoryId);
            }
            catch
            {
                throw new InvalidCategoryException($"Category with ID {product.CategoryId} not found");
            }
            
            _repository.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            ValidateProduct(product);
            _repository.Update(product);
        }

        public void DeleteProduct(string id)
        {
            _repository.Delete(id);
        }

        public void UpdateStock(string productId, int quantityChange)
        {
            var product = GetProductById(productId);
            var newQuantity = product.QuantityInStock + quantityChange;
            
            if (newQuantity < 0)
                throw new InsufficientStockException($"Insufficient stock for product {product.Name}");
            
            product.QuantityInStock = newQuantity;
            product.LastRestocked = System.DateTime.Now;
            _repository.Update(product);
        }

        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new InvalidProductException("Product name cannot be empty");
            
            if (product.Price <= 0)
                throw new InvalidProductException("Product price must be positive");
            
            if (product.QuantityInStock < 0)
                throw new InvalidProductException("Stock quantity cannot be negative");
        }
    }
}
