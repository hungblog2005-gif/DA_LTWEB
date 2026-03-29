// File: Controllers/CartController.cs

using DA_WEB.Models; // Nếu cần ApplicationUser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]/[action]")] // Dễ nhớ route, ví dụ: /Cart/Index
public class CartController : Controller
{

    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager; // Để lấy UserId nếu đăng nhập

    public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
    {
        _cartService = cartService;
        _userManager = userManager;
    }

    // GET: /Cart/Index
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User); // Lấy UserId nếu đăng nhập, nếu không thì null
        var cart = await _cartService.GetCartAsync(userId, HttpContext); // Gửi HttpContext để CartService xử lý Session nếu cần
        var totalPrice = await _cartService.GetTotalPriceAsync(userId, HttpContext);

        ViewBag.TotalPrice = totalPrice;
        return View(cart); // Truyền model giỏ hàng tới View
    }

    // POST: /Cart/Add
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string size = "")
    {
        var userId = _userManager.GetUserId(User);
        // Truyền thêm size vào Service
        await _cartService.AddToCartAsync(productId, quantity, size, userId, HttpContext);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Update
    [HttpPost]
    public async Task<IActionResult> Update(int productId, int quantity, string size)
    {
        var userId = _userManager.GetUserId(User);
        await _cartService.UpdateQuantityAsync(productId, quantity, size, userId, HttpContext);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Remove
    [HttpPost]
    public async Task<IActionResult> Remove(int productId, string size)
    {
        var userId = _userManager.GetUserId(User);
        await _cartService.RemoveFromCartAsync(productId, size, userId, HttpContext);
        return RedirectToAction(nameof(Index));
    }
}