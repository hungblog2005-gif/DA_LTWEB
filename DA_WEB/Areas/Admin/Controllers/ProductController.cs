// Areas/Admin/Controllers/ProductController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DA_WEB.Data;
using DA_WEB.Models;

namespace DA_WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            ModelState.Remove("Category");
            ModelState.Remove("CategoryId");

            // ← THÊM ĐOẠN NÀY ĐỂ XEM LỖI
            Console.WriteLine("=== MODELSTATE DEBUG ===");
            Console.WriteLine($"IsValid: {ModelState.IsValid}");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state!.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                        Console.WriteLine($"  LOI [{key}]: {error.ErrorMessage}");
                }
                else
                {
                    Console.WriteLine($"  OK  [{key}]: {state.RawValue ?? "(null)"}");
                }
            }
            Console.WriteLine("========================");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadPath);
                using var stream = new FileStream(
                    Path.Combine(uploadPath, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/images/products/" + fileName;
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(
                _db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(
                _db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            // Loại bỏ navigation property khỏi validation
            ModelState.Remove("Category");
            ModelState.Remove("CategoryId");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadPath);
                using var stream = new FileStream(
                    Path.Combine(uploadPath, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                product.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                // Giữ lại ImageUrl cũ nếu không upload ảnh mới
                var existing = await _db.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (existing != null)
                    product.ImageUrl = existing.ImageUrl;
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = (await _db.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id))?.CreatedAt ?? DateTime.Now;

                _db.Update(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Debug
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state!.Errors)
                    Console.WriteLine($"[VALIDATION] {key}: {error.ErrorMessage}");
            }

            ViewBag.Categories = new SelectList(
                _db.Categories, "Id", "Name", product.CategoryId);
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
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Xóa ảnh khỏi server nếu có
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imgPath = Path.Combine(_env.WebRootPath,
                    product.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}