using Azure.Core.Pipeline;
using DA_WEB.Data;
using DA_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DA_WEB.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        // Tiêm AppDbContext vào để có thể gọi dữ liệu từ SQL Server
        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        // Đổi thành async để lấy sản phẩm bất đồng bộ (không làm đơ trang)
        public async Task<IActionResult> Index()
        {// 1. Lấy 4 sản phẩm mới nhất
            var newProducts = await _db.Products
                .Include(p => p.ProductReviews) // <-- THÊM DÒNG NÀY ĐỂ KÉO ĐÁNH GIÁ
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            // 2. Lấy 3 sản phẩm nổi bật
            ViewBag.FeaturedProducts = await _db.Products
                .Include(p => p.ProductReviews) // <-- THÊM DÒNG NÀY ĐỂ KÉO ĐÁNH GIÁ
                .Where(p => p.IsActive)
                .OrderBy(p => p.Id)
                .Take(3)
                .ToListAsync();

            return View(newProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult PrivacyPolicy()
        {
            return View(); // Trả về Views/Home/PrivacyPolicy.cshtml
        }

        public IActionResult TermsOfService()
        {
            return View(); // Trả về Views/Home/TermsOfService.cshtml
        }

        public IActionResult ReturnPolicy()
        {
            return View(); // Trả về Views/Home/ReturnPolicy.cshtml
        }

        public IActionResult ShippingPolicy()
        {
            return View();
        }

        public IActionResult SizeGuide()
        {
            return View(); // Trả về Views/Home/SizeGuide.cshtml
        }

        public IActionResult Contact()
        {
            return View(); // Trả về Views/Home/Contact.cshtml
        }

        public IActionResult Philosophy()
        {
            return View(); // Trả về Views/Home/Philosophy.cshtml
        }

        public IActionResult About()
        {
            return View(); // Trả về Views/Home/About.cshtml
        }

        public IActionResult FAQ()
        {
            return View(); // Trả về Views/Home/FAQ.cshtml
        }
    }
}