using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DA_WEB.Data;
using DA_WEB.Models;

namespace DA_WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel();

            // 1. KPI CARDS
            // Giả định trạng thái thành công là "Paid" hoặc chọn COD (Pending nhưng vẫn tính là đơn). 
            // Bạn có thể chỉnh lại chữ "Paid" cho khớp với VNPay của bạn sau.
            vm.TotalRevenue = await _db.Orders
                .Where(o => o.Status == "Paid" || o.PaymentMethod == "COD")
                .SumAsync(o => o.TotalAmount);

            vm.TotalOrders = await _db.Orders.CountAsync();
            vm.TotalProducts = await _db.Products.CountAsync();

            // Đếm số lượng User mang Role Customer (Nếu không phân Role thì dùng CountAsync toàn bộ)
            vm.TotalCustomers = await _userManager.Users.CountAsync();

            // 2. ALERTS (Cảnh báo đỏ)
            vm.LowStockProducts = await _db.Products
                .Where(p => p.Stock <= 5)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            vm.PendingOrders = await _db.Orders
                .Where(o => o.Status == "Pending")
                .OrderBy(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // 3. RECENT ORDERS (5 Đơn hàng mới nhất)
            vm.RecentOrders = await _db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // 4. TOP SẢN PHẨM BÁN CHẠY (Thay thế cho Biểu đồ)
            vm.TopProducts = await _db.Orders
                .Where(o => o.Status != "Cancelled") // Bỏ qua đơn bị hủy
                .SelectMany(o => o.Items)            // Moi tất cả sản phẩm trong các đơn ra
                .GroupBy(i => i.Product.Name)        // Gom nhóm theo Tên sản phẩm
                .Select(g => new TopProductModel
                {
                    ProductName = g.Key ?? "Unknown",
                    TotalSold = g.Sum(i => i.Quantity), // Cộng dồn số lượng bán
                    Revenue = g.Sum(i => i.Quantity * i.UnitPrice) // Cộng dồn tiền thu được
                })
                .OrderByDescending(x => x.TotalSold) // Xếp từ cao xuống thấp
                .Take(5)                             // Lấy 5 chiếc áo top đầu
                .ToListAsync();

            return View(vm);
        }

    }
}