using DA_WEB.Models;
using Microsoft.AspNetCore.Http;
using DA_WEB.Data;
public interface ICartService
{
    Task<Cart?> GetCartAsync(string? userId, HttpContext context);
    Task AddToCartAsync(int productId, int quantity, string? userId, HttpContext context);
    Task UpdateQuantityAsync(int productId, int quantity, string? userId, HttpContext context);
    Task RemoveFromCartAsync(int productId, string? userId, HttpContext context);
    Task ClearCartAsync(string? userId, HttpContext context);
    Task<decimal> GetTotalPriceAsync(string? userId, HttpContext context);
}