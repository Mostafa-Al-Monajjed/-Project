using System.Collections.Generic;
using StoreManagement.Models;
using StoreManagement.Interfaces;
using StoreManagement.Exceptions;

namespace StoreManagement.Services
{
    public class CategoryService
    {
        private readonly IFileRepository<Category> _repository;

        public CategoryService(IFileRepository<Category> repository)
        {
            _repository = repository;
        }

        public List<Category> GetAllCategories()
        {
            return _repository.LoadAll();
        }

        public Category GetCategoryById(string id)
        {
            var category = _repository.GetById(id);
            if (category == null)
                throw new InvalidCategoryException($"Category with ID {id} not found");
            return category;
        }

        public void AddCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new InvalidCategoryException("Category name cannot be empty");
            
            _repository.Add(category);
        }

        public void UpdateCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new InvalidCategoryException("Category name cannot be empty");
            
            _repository.Update(category);
        }

        public void DeleteCategory(string id)
        {
            _repository.Delete(id);
        }
    }
}
