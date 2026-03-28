using DA_WEB.Models;
using Microsoft.AspNetCore.Http;
using DA_WEB.Data;
public interface ICartService
{
    Task<Cart?> GetCartAsync(string? userId, HttpContext context);
    Task AddToCartAsync(int productId, int quantity, string? size, string? userId, HttpContext httpContext);
    Task UpdateQuantityAsync(int productId, int quantity, string? size, string? userId, HttpContext httpContext);
    Task RemoveFromCartAsync(int productId, string? size, string? userId, HttpContext httpContext);
    Task ClearCartAsync(string? userId, HttpContext context);
    Task<decimal> GetTotalPriceAsync(string? userId, HttpContext context);
}