using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StoreManagement.Models;
using StoreManagement.Interfaces;
using StoreManagement.Exceptions;
using StoreManagement.Utils;

namespace StoreManagement.Services
{
    public class UserService
    {
        private readonly IFileRepository<User> _repository;
        private User? _currentUser;

        public UserService(IFileRepository<User> repository)
        {
            _repository = repository;
        }

        public User CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;

        public bool Login(string username, string password)
        {
            var users = _repository.LoadAll();
            var user = users.FirstOrDefault(u => 
                u.Username == username && u.Password == password && u.IsActive);
            
            if (user == null)
            {
                throw new LoginFailedException("Invalid username or password");
            }

            _currentUser = user;
            return true;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public void CreateUser(User user, User creator)
        {
            if (creator.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can create users");
            
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                throw new InvalidInputException("Username and password are required");
            

            if(!Regex.IsMatch(user.Username, @"^[a-zA-Z0-9_]{3,20}$"))
                throw new InvalidInputException("Username must be 3-20 characters and contain only letters, numbers, and underscores");

            var users = _repository.LoadAll();
            if (users.Any(u => u.Username == user.Username))
                throw new InvalidInputException("Username already exists");
            
            _repository.Add(user);
        }

        public void UpdateUser(User user, User updater)
        {
            if (updater.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can update users");
            
            _repository.Update(user);
        }

        public void DeleteUser(string userId, User deleter)
        {
            if (deleter.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete users");
            
            if (deleter.Id == userId)
                throw new InvalidInputException("Cannot delete your own account");
            
            _repository.Delete(userId);
        }

        public List<User> GetAllUsers(User requester)
        {
            if (requester.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can view all users");
            
            return _repository.LoadAll();
        }

        public User GetUserById(string id, User requester)
        {
            if (requester.Role != UserRole.Admin && requester.Id != id)
                throw new UnauthorizedAccessException("Access denied");
            
            return _repository.GetById(id);
        }

        public void InitializeAdminUser()
        {
            var users = _repository.LoadAll();
            if (!users.Any(u => u.Role == UserRole.Admin))
            {
                var admin = new User()
                {
                    Username = "admin",
                    Password = "password",
                    FullName = "Administrator",
                    Role = UserRole.Admin
                };
                _repository.Add(admin);
            }
        }
    }
}
