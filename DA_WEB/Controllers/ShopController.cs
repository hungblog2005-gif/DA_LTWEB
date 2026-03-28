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
        public async Task<IActionResult> Index(int? categoryId, decimal? maxPrice, string? sortOrder, string? size)
        {
            // Bắt đầu với truy vấn lấy tất cả sản phẩm đang hoạt động
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // 1. Lọc theo Danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // 2. Lọc theo Khoảng giá (Ưu tiên giá Sale nếu có, không thì lấy giá gốc)
            if (maxPrice.HasValue && maxPrice > 0)
            {
                query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice);
            }

            // 3. Lọc theo Size (Tạm comment lại - Chỉ mở khóa nếu Model Product của bạn có cột Size)
            // if (!string.IsNullOrEmpty(size))
            // {
            //     query = query.Where(p => p.Size == size);
            // }

            // 4. Sắp xếp (Sorting)
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.SalePrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
                "popular" => query.OrderByDescending(p => p.Id), // Tạm dùng Id làm tiêu chí phổ biến
                _ => query.OrderByDescending(p => p.CreatedAt) // Mặc định là Mới nhất
            };

            var products = await query.ToListAsync();

            // Lưu lại các lựa chọn vào ViewBag để View giữ nguyên trạng thái hiển thị
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MaxPrice = maxPrice ?? 5000000; // Mặc định hiển thị mốc 5 triệu
            ViewBag.SortOrder = sortOrder;
            ViewBag.SelectedSize = size;

            return View(products);
        }

        // GET: /Shop/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
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