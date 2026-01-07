using System.Collections.Generic;

namespace StoreManagement.Interfaces
{
    public interface IFileRepository<T>
    {
        List<T> LoadAll();
        T GetById(string id);
        void Add(T item);
        void Update(T item);
        void Delete(string id);
        void SaveAll(List<T> items);
    }
}