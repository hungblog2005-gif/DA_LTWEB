using DA_WEB.Data;
using DA_WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DA_WEB.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShopController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Shop
        // Tiến nhớ copy TỪ DÒNG NÀY (để cập nhật tham số nhé)
        public async Task<IActionResult> Index(int? categoryId, string sortOrder, string size, string color, string searchString, decimal? maxPrice)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            // 1. Lọc theo Tên (Search)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            }

            // 2. Lọc theo Danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // 3. Lọc theo Giá
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. Lọc theo Size và Color (TẠM THỜI ĐÓNG BĂNG VÌ DATABASE CHƯA CÓ)
            /*
            if (!string.IsNullOrEmpty(size))
            {
                // query = query.Where(p => p.Size == size); 
            }
            if (!string.IsNullOrEmpty(color))
            {
                // query = query.Where(p => p.Color == color);
            }
            */

            // 5. Sắp xếp (Sort)
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "popular":
                    query = query.OrderByDescending(p => p.Id);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // 6. Lưu trạng thái về View
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SelectedSize = size;
            ViewBag.SelectedColor = color;
            ViewBag.MaxPrice = maxPrice ?? 5000000;

            var products = await query.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // 1. Lấy danh sách đánh giá đã có
            var reviews = await _db.ProductReviews
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            ViewBag.Reviews = reviews;

            // 2. Kiểm tra xem User hiện tại có quyền đánh giá không (Đã mua + Delivered)
            bool canReview = false;
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                canReview = await _db.Orders
                    .AnyAsync(o => o.UserId == userId &&
                                   o.Status == "Delivered" &&
                                   o.Items.Any(i => i.ProductId == id));
            }
            ViewBag.CanReview = canReview;

            return View(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostReview(int productId, int rating, string comment)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra lại quyền (Security check)
            var hasPurchased = await _db.Orders
                .AnyAsync(o => o.UserId == userId &&
                               o.Status == "Delivered" &&
                               o.Items.Any(i => i.ProductId == productId));

            if (hasPurchased)
            {
                var review = new ProductReview
                {
                    ProductId = productId,
                    UserId = userId!,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };
                _db.ProductReviews.Add(review);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { id = productId });
        }
    }
}