using System;

namespace StoreManagement.Models
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
      //  public bool IsActive { get; set; }

        public Category(string Name, string Description)
        {
            Id = Guid.NewGuid().ToString();
            this.Name = Name;
            this.Description = Description;
            CreatedAt = DateTime.Now;
            //IsActive = true;
        }

        public Category()
        {
        }
    }
}
