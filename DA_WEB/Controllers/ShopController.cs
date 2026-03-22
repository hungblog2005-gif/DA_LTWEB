// Controllers/ShopController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DA_WEB.Data;

namespace DA_WEB.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _db;

        public ShopController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Shop
        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
                ViewBag.SelectedCategory = categoryId;
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _db.Categories.ToListAsync();

            return View(products);
        }

        // GET: /Shop/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            // Related products — cùng category, trừ product hiện tại
            var related = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Id != id
                    && (product.CategoryId == null || p.CategoryId == product.CategoryId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = related;

            return View(product);
        }
    }
}
