using DA_WEB.Areas.Admin.Models;
using DA_WEB.Data;
using DA_WEB.Models; // Chứa ApplicationUser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DA_WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Admin/Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _db.Users
                .Select(u => new CustomerViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    TotalOrders = _db.Orders.Count(o => o.UserId == u.Id),
                    TotalSpent = _db.Orders.Where(o => o.UserId == u.Id && o.Status != "Cancelled")
                                           .Sum(o => (decimal?)o.TotalAmount) ?? 0,
                    // Nếu thời gian khóa vẫn còn trong tương lai thì coi như đang bị khóa
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTime.Now
                })
                .OrderByDescending(c => c.TotalSpent)
                .ToListAsync();

            return View(customers);
        }

        // POST: Admin/Customer/ToggleLock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // Đang khóa -> Mở khóa
                user.LockoutEnd = null;
                TempData["SuccessMessage"] = $"Đã mở khóa tài khoản {user.Email}";
            }
            else
            {
                // Đang tự do -> Khóa vĩnh viễn (100 năm)
                user.LockoutEnd = DateTime.Now.AddYears(100);
                TempData["ErrorMessage"] = $"Đã khóa tài khoản {user.Email}";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Customer/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Ràng buộc: Nếu khách đã có đơn hàng thì không cho xóa (để giữ lịch sử database)
            var hasOrders = await _db.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng đã có lịch sử đơn hàng. Hãy dùng chức năng Khóa.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                TempData["SuccessMessage"] = "Đã xóa khách hàng thành công.";
            else
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa.";

            return RedirectToAction(nameof(Index));
        }
    }
}