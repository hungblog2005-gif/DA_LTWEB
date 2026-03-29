using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DA_WEB.Data;
using DA_WEB.Models;

namespace DA_WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // (Nếu có lỗi ở SD thì bạn hãy kiểm tra lại file chứa hằng số nhé)
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Admin/Product
        public async Task<IActionResult> Index()
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        // GET: /Admin/Product/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View(new Product());
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles)
        {
            ModelState.Remove("Category");
            ModelState.Remove("CategoryId");
            ModelState.Remove("ProductImages"); // Vô hiệu hóa validate list ảnh phụ

            // 1. Xử lý Ảnh đại diện (Main Image)
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadPath);
                using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/images/products/" + fileName;
            }

            // 2. Xử lý Danh sách ảnh phụ (Gallery) - Nhận nhiều file cùng lúc
            if (galleryFiles != null && galleryFiles.Count > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadPath);
                foreach (var file in galleryFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                        await file.CopyToAsync(stream);

                        // Add thẳng ảnh vào danh sách của Product
                        product.ProductImages.Add(new ProductImage { ImageUrl = "/images/products/" + fileName });
                    }
                }
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Lấy cả danh sách ảnh phụ lên để View có thể hiển thị
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("CategoryId");
            ModelState.Remove("ProductImages");

            // 1. Cập nhật Ảnh đại diện
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadPath);
                using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                var existing = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (existing != null) product.ImageUrl = existing.ImageUrl;
            }

            // 2. Tải thêm Ảnh phụ mới
            if (galleryFiles != null && galleryFiles.Count > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadPath);
                foreach (var file in galleryFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                        await file.CopyToAsync(stream);

                        // Lưu trực tiếp vào Database
                        _db.ProductImages.Add(new ProductImage { ProductId = product.Id, ImageUrl = "/images/products/" + fileName });
                    }
                }
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = (await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id))?.CreatedAt ?? DateTime.Now;
                _db.Update(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Lấy kèm ảnh phụ để dọn rác ổ cứng
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // 1. Xóa file ảnh đại diện khỏi server
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imgPath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(imgPath)) System.IO.File.Delete(imgPath);
            }

            // 2. Xóa toàn bộ file ảnh phụ khỏi server
            foreach (var img in product.ProductImages)
            {
                if (!string.IsNullOrEmpty(img.ImageUrl))
                {
                    var galleryImgPath = Path.Combine(_env.WebRootPath, img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(galleryImgPath)) System.IO.File.Delete(galleryImgPath);
                }
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}