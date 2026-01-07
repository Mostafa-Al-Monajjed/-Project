using System;

namespace StoreManagement.Models
{
    public enum UserRole
    {
        Admin,
        Employee
    }

    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
       // public string FullName { get; set; }
        public UserRole Role { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; internal set; }

        public User(string Username, string Password)
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            this.Username = Username;
            this.Password = Password;   
            IsActive = true;
        }

        public User()
        {
        }
    }
}
