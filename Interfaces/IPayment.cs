using StoreManagement.Models;

namespace StoreManagement.Interfaces
{
    public interface IPayment
    {
        bool ProcessPayment(Order order, decimal amount);
        string GenerateReceipt(Order order);
    }
}