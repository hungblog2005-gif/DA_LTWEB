using DA_WEB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
            // Nếu là khách (guest), dùng SessionId
            cartId = GetOrCreateSessionId(httpContext);
        }
        else
        {
            // Nếu là người dùng đã đăng nhập, dùng UserId
            cartId = userId;
        }

        // Lấy giỏ hàng từ DB, bao gồm các mục giỏ và thông tin sản phẩm
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

        public async Task AddToCartAsync(int productId, int quantity, string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart == null)
        {
            // Tạo giỏ hàng mới
            cart = new Cart
            {
                    // Nếu là guest dùng SessionId, nếu là user thì dùng UserId làm Id để nhất quán với GetCartAsync
                    Id = string.IsNullOrEmpty(userId) ? GetOrCreateSessionId(httpContext) : userId!,
                UserId = userId // Gán UserId nếu có
            };
            _context.Carts.Add(cart);
        }

        // Kiểm tra xem sản phẩm đã có trong giỏ chưa
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            // Nếu đã có, cộng dồn số lượng
            existingItem.Quantity += quantity;
        }
        else
        {
            // Nếu chưa có, thêm mới vào giỏ
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price // Lưu giá tại thời điểm thêm vào giỏ để tránh thay đổi sau này
                });
            }
            else
            {
                // Nếu sản phẩm không tồn tại, có thể ném ngoại lệ hoặc bỏ qua
                // Ví dụ: throw new ArgumentException($"Product with ID {productId} not found.");
                return; // Bỏ qua nếu không tìm thấy sản phẩm
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(int productId, int quantity, string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart != null)
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    // Nếu số lượng <= 0, xóa sản phẩm khỏi giỏ
                    cart.Items.Remove(item);
                }
                else
                {
                    // Cập nhật số lượng
                    item.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveFromCartAsync(int productId, string? userId, HttpContext httpContext)
    {
        // Gọi hàm UpdateQuantity với quantity = 0 để xóa sản phẩm
        await UpdateQuantityAsync(productId, 0, userId, httpContext);
    }

    public async Task ClearCartAsync(string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart != null)
        {
            // Xóa tất cả các mục trong giỏ
            _context.CartItems.RemoveRange(cart.Items);
            // Xóa chính giỏ hàng
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalPriceAsync(string? userId, HttpContext httpContext)
    {
        var cart = await GetCartAsync(userId, httpContext);
        if (cart != null)
        {
            // Tính tổng giá trị: Sum(Quantity * UnitPrice)
            return cart.Items.Sum(item => item.Quantity * item.UnitPrice);
        }
        return 0; // Trả về 0 nếu không có giỏ hoặc giỏ trống
    }

    /// <summary>
    /// Hàm hỗ trợ để lấy hoặc tạo SessionId cho khách.
    /// </summary>
    /// <param name="contextuy cập Session.</param>
    /// <returns>SessionId duy nhất cho phiên làm việc của khách.</returns>
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