using DA_WEB.Models;
public interface IOrderService
{
    Task<int> CreateOrderAsync(
        string userId, string fullName, string phone,
        string address, string email, string paymentMethod);

    Task<Order?> GetOrderByIdAsync(int orderId);

    Task UpdateOrderStatusAsync(int orderId, string status, string? stripeId = null);

    Task UpdateStripeIntentAsync(int orderId, string paymentIntentId);
}