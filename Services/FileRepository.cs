using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using StoreManagement.Interfaces;
using StoreManagement.Utils;

namespace StoreManagement.Services
{
    public sealed class FileRepository<T> : IFileRepository<T>
    {
        private readonly string _filePath;
        private List<T> _items;

        public FileRepository(string filePath)
        {
            if (!Regex.IsMatch(filePath, @"\.json$", RegexOptions.IgnoreCase))
            {
                throw new Exceptions.FileFormatException("Invalid file format. Only JSON files are supported.");
            }

            _filePath = filePath;
            _items = new List<T>();
            EnsureFileExists();
            LoadAll();
        }

        private void EnsureFileExists()
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public List<T> LoadAll()
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                _items = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                return _items;
            }
            catch (Exception ex)
            {
                throw new Exceptions.FileFormatException($"Failed to load data: {ex.Message}");
            }
        }

        public T GetById(string id)
        {
            var item = _items.Find(i => 
            {
                var prop = i.GetType().GetProperty("Id");
                return prop != null && prop.GetValue(i)?.ToString() == id;
            });
            return item;
        }

        public void Add(T item)
        {
            _items.Add(item);
            SaveAll(_items);
        }

        public void Update(T item)
        {
            var prop = item.GetType().GetProperty("Id");
            if (prop == null) return;
            
            var id = prop.GetValue(item)?.ToString();
            var index = _items.FindIndex(i => 
            {
                var iProp = i.GetType().GetProperty("Id");
                return iProp != null && iProp.GetValue(i)?.ToString() == id;
            });
            
            if (index >= 0)
            {
                _items[index] = item;
                SaveAll(_items);
            }
        }

        public void Delete(string id)
        {
            _items.RemoveAll(item => 
            {
                var prop = item.GetType().GetProperty("Id");
                return prop != null && prop.GetValue(item)?.ToString() == id;
            });
            SaveAll(_items);
        }

        public void SaveAll(List<T> items)
        {
            try
            {
                var json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(_filePath, json);
                _items = items;
            }
            catch (Exception ex)
            {
                throw new Exceptions.FileFormatException($"Failed to save data: {ex.Message}");
            }
        }
    }
}
