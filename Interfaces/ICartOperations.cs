using StoreManagement.Models;

namespace StoreManagement.Interfaces
{
    public interface ICartOperations
    {
        void AddItem(Product product, int quantity);
        void RemoveItem(string productId);
        void UpdateQuantity(string productId, int quantity);
        void ClearCart();
        decimal CalculateTotal();
        Order Checkout(string customerName, string phone, PaymentMethod paymentMethod);
    }
}