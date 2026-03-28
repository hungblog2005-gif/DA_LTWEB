using DA_WEB.Models;
using DA_WEB.Services; // Thêm dòng này để gọi được IVnPayService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace DA_WEB.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StripeSettings _stripeSettings;

        // 1. Khai báo thêm Service VNPay
        private readonly IVnPayService _vnPayService;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            IOptions<StripeSettings> stripeSettings,
            IVnPayService vnPayService) // 2. Tiêm vào constructor
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;
            _vnPayService = vnPayService;
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
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Where(x => x.Value!.Errors.Any())
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value!.Errors.Select(e => e.ErrorMessage))}");
                TempData["ErrorMessage"] = "Validation lỗi: " + string.Join(" | ", errorList);
                return View("Checkout", orderDetails);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            try
            {
                var orderId = await _orderService.CreateOrderAsync(
                    userId, orderDetails.FullName, orderDetails.Phone,
                    orderDetails.Address, orderDetails.Email, orderDetails.PaymentMethod
                );

                // NHÁNH 1: THANH TOÁN STRIPE
                if (orderDetails.PaymentMethod == "Stripe")
                    return RedirectToAction("StripePayment", new { orderId });

                // NHÁNH 2: THANH TOÁN VNPAY
                if (orderDetails.PaymentMethod == "VnPay")
                {
                    // Lấy lại order để biết tổng tiền cần thanh toán
                    var order = await _orderService.GetOrderByIdAsync(orderId);

                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = (double)order!.TotalAmount,
                        CreatedDate = DateTime.Now,
                        Description = $"{orderDetails.FullName} thanh toan don hang {orderId}",
                        FullName = orderDetails.FullName,
                        OrderId = orderId
                    };

                    // Tạo URL của VNPay và chuyển hướng khách hàng sang đó
                    var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);
                    return Redirect(paymentUrl);
                }

                // NHÁNH 3: THANH TOÁN COD (Nhận hàng trả tiền)
                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{orderId}";
                return RedirectToAction("Success", new { id = orderId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Checkout", orderDetails);
            }
        }

        // ─── GET /Order/PaymentCallBack (VNPAY TRẢ KẾT QUẢ VỀ ĐÂY) ─────────
        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["ErrorMessage"] = $"Lỗi thanh toán VNPay. Mã lỗi: {response?.VnPayResponseCode}";
                return RedirectToAction("Index", "Cart");
            }

            // Thanh toán VNPay thành công (ResponseCode == "00")
            int orderId = Convert.ToInt32(response.OrderId);
            var userId = _userManager.GetUserId(User);

            // Cập nhật trạng thái đơn hàng và xóa giỏ hàng
            await _orderService.UpdateOrderStatusAsync(orderId, "Paid", response.TransactionId);
            await _cartService.ClearCartAsync(userId!, null);

            ViewBag.OrderId = orderId;
            TempData["SuccessMessage"] = $"Thanh toán VNPay thành công! Mã đơn hàng: #{orderId}";
            return View("Success");
        }

        // (Các hàm StripePayment, PaymentSuccess, PaymentCancel, Success giữ nguyên như cũ của bạn...)

        [HttpGet]
        public async Task<IActionResult> StripePayment(int orderId)
        {
            /* Code Stripe của bạn giữ nguyên */
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _cartService.GetCartAsync(user.Id, null);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound();

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)order.TotalAmount,
                Currency = "vnd",
                Metadata = new Dictionary<string, string> { { "order_id", orderId.ToString() }, { "user_id", user.Id } }
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

        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(int orderId, string? payment_intent)
        {
            /* Code Stripe Callback của bạn giữ nguyên */
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

                        ViewBag.OrderId = orderId;
                        TempData["SuccessMessage"] = $"Thanh toán thành công! Mã đơn hàng: #{orderId}";
                        return View("Success");
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

        [HttpGet]
        public async Task<IActionResult> PaymentCancel(int orderId)
        {
            if (orderId > 0) await _orderService.UpdateOrderStatusAsync(orderId, "Cancelled");
            TempData["ErrorMessage"] = "Thanh toán đã bị hủy.";
            return RedirectToAction("Index", "Cart");
        }

        [HttpGet]
        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // ─── GET /Order/History ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Giả sử IOrderService của bạn có hàm GetUserOrdersAsync để lấy danh sách đơn
            // Nếu hàm của bạn tên khác, hãy đổi lại cho khớp nhé!
            var orders = await _orderService.GetUserOrdersAsync(userId);

            return View(orders);
        }
    }
}