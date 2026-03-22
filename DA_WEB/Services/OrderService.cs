using DA_WEB.Data;
using DA_WEB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICartService _cartService;

    public OrderService(AppDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    // Tạo đơn hàng — giữ nguyên logic cũ của bạn
    public async Task<int> CreateOrderAsync(
        string userId, string fullName, string phone,
        string address, string email, string paymentMethod)
    {
        var cart = await _cartService.GetCartAsync(userId, null);

        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Giỏ hàng trống.");

        var order = new Order
        {
            UserId = userId,
            FullName = fullName,
            Phone = phone,
            Address = address,
            Email = email,
            PaymentMethod = paymentMethod,
            TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice),
            Status = paymentMethod == "Stripe" ? "AwaitingPayment" : "Pending",
            OrderDate = DateTime.UtcNow
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // COD: xoá giỏ ngay. Stripe: xoá sau khi thanh toán xong
        if (paymentMethod != "Stripe")
            await _cartService.ClearCartAsync(userId, null);

        return order.Id;
    }

    // Lấy order theo ID
    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    // Cập nhật trạng thái
    public async Task UpdateOrderStatusAsync(int orderId, string status, string? stripeId = null)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return;

        order.Status = status;
        if (stripeId != null) order.StripeSessionId = stripeId;

        await _context.SaveChangesAsync();
    }

    // Lưu PaymentIntent ID
    public async Task UpdateStripeIntentAsync(int orderId, string paymentIntentId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return;

        order.StripeSessionId = paymentIntentId;
        await _context.SaveChangesAsync();
    }
}