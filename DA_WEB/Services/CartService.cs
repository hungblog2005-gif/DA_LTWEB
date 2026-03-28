using DA_WEB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DA_WEB.Data;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;

    public CartService(AppDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<Cart?> GetCartAsync(string? userId, HttpContext httpContext)
    {
        string cartId;

        if (string.IsNullOrEmpty(userId))
        {
            cartId = GetOrCreateSessionId(httpContext);
        }
        else
        {
            cartId = userId;
        }

        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

    // ĐÃ THÊM THAM SỐ SIZE VÀ LOGIC TÁCH DÒNG NẾU KHÁC SIZE
    public async Task AddToCartAsync(int productId, int quantity, string? size, string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart == null)
        {
            cart = new Cart
            {
                Id = string.IsNullOrEmpty(userId) ? GetOrCreateSessionId(httpContext) : userId!,
                UserId = userId
            };
            _context.Carts.Add(cart);
        }

        // Kiểm tra xem sản phẩm đã có trong giỏ chưa (PHẢI TRÙNG CẢ ID VÀ SIZE)
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Size = size, // Lưu size khách chọn vào Database
                    UnitPrice = product.Price
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    // ĐÃ THÊM THAM SỐ SIZE
    public async Task UpdateQuantityAsync(int productId, int quantity, string? size, string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart != null)
        {
            // Tìm đúng áo VÀ đúng size
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
            }
        }
    }

    // ĐÃ THÊM THAM SỐ SIZE
    public async Task RemoveFromCartAsync(int productId, string? size, string? userId, HttpContext httpContext)
    {
        await UpdateQuantityAsync(productId, 0, size, userId, httpContext);
    }

    // HÀM BỊ MẤT ĐÃ ĐƯỢC THÊM LẠI
    public async Task ClearCartAsync(string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart != null)
        {
            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
    }

    // HÀM BỊ MẤT ĐÃ ĐƯỢC THÊM LẠI
    public async Task<decimal> GetTotalPriceAsync(string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart != null)
        {
            return cart.Items.Sum(item => item.Quantity * item.UnitPrice);
        }
        return 0;
    }

    // HÀM BỊ MẤT ĐÃ ĐƯỢC THÊM LẠI
    private string GetOrCreateSessionId(HttpContext context)
    {
        var sessionId = context.Session.GetString("SessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            context.Session.SetString("SessionId", sessionId);
        }
        return sessionId;
    }
}