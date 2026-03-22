// File: Controllers/OrderController.cs

using DA_WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly StripeSettings _stripeSettings;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        UserManager<ApplicationUser> userManager,
        IOptions<StripeSettings> stripeSettings)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userManager = userManager;
        _stripeSettings = stripeSettings.Value;
    }

    // ─── GET /Order/Checkout ──────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = new Order
        {
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
        };

        return View(order);
    }

    // ─── POST /Order/Confirm ──────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Order orderDetails)
    {
        // ── DEBUG: xoá sau khi fix xong ──────────────────────────────────
        if (!ModelState.IsValid)
        {
            var errorList = ModelState
                .Where(x => x.Value!.Errors.Any())
                .Select(x => $"{x.Key}: {string.Join(", ", x.Value!.Errors.Select(e => e.ErrorMessage))}");

            // Hiện lỗi ngay trên trang
            TempData["ErrorMessage"] = "Validation lỗi: " + string.Join(" | ", errorList);
            return View("Checkout", orderDetails);
        }
        // ─────────────────────────────────────────────────────────────────

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        try
        {
            var orderId = await _orderService.CreateOrderAsync(
                userId,
                orderDetails.FullName,
                orderDetails.Phone,
                orderDetails.Address,
                orderDetails.Email,
                orderDetails.PaymentMethod
            );

            if (orderDetails.PaymentMethod == "Stripe")
                return RedirectToAction("StripePayment", new { orderId });

            TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{orderId}";
            return RedirectToAction("Success", new { id = orderId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Checkout", orderDetails);
        }
    }

    // ─── GET /Order/StripePayment ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> StripePayment(int orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var cart = await _cartService.GetCartAsync(user.Id, null);
        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null) return NotFound();

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)order.TotalAmount,
            Currency = "vnd",
            Metadata = new Dictionary<string, string>
            {
                { "order_id", orderId.ToString() },
                { "user_id",  user.Id }
            }
        };

        var service = new PaymentIntentService();
        var intent = service.Create(options);

        await _orderService.UpdateStripeIntentAsync(orderId, intent.Id);

        ViewBag.ClientSecret = intent.ClientSecret;
        ViewBag.PublishableKey = _stripeSettings.PublishableKey;
        ViewBag.CartItems = cart.Items;
        ViewBag.TotalAmount = order.TotalAmount;
        ViewBag.ReturnUrl = $"{Request.Scheme}://{Request.Host}/Order/PaymentSuccess?orderId={orderId}";

        return View();
    }

    // ─── GET /Order/PaymentSuccess ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> PaymentSuccess(int orderId, string? payment_intent)
    {
        try
        {
            if (!string.IsNullOrEmpty(payment_intent))
            {
                StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
                var service = new PaymentIntentService();
                var intent = service.Get(payment_intent);

                if (intent.Status == "succeeded")
                {
                    var userId = _userManager.GetUserId(User);
                    await _orderService.UpdateOrderStatusAsync(orderId, "Paid", payment_intent);
                    await _cartService.ClearCartAsync(userId!, null);

                    // ← FIX: truyền orderId vào ViewBag trực tiếp, không redirect
                    ViewBag.OrderId = orderId;
                    TempData["SuccessMessage"] = $"Thanh toán thành công! Mã đơn hàng: #{orderId}";
                    return View("Success"); // ← trả về Success view trực tiếp
                }
            }

            TempData["ErrorMessage"] = "Thanh toán chưa hoàn tất.";
            return RedirectToAction("StripePayment", new { orderId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            return RedirectToAction("StripePayment", new { orderId });
        }
    }

    // ─── GET /Order/PaymentCancel ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> PaymentCancel(int orderId)
    {
        if (orderId > 0)
            await _orderService.UpdateOrderStatusAsync(orderId, "Cancelled");

        TempData["ErrorMessage"] = "Thanh toán đã bị hủy.";
        return RedirectToAction("Index", "Cart");
    }

    // ─── GET /Order/Success ───────────────────────────────────────────────
    [HttpGet]
    public IActionResult Success(int id)
    {
        ViewBag.OrderId = id;
        return View();
    }
}