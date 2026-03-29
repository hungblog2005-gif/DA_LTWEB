using DA_WEB.Data;
using DA_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DA_WEB.Controllers.Api
{
    [Route("api/products")]
    [ApiController] // Đánh dấu đây là API Controller
    public class ProductApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductApiController(AppDbContext db)
        {
            _db = db;
        }

        // 1. GET: /api/products (Lấy danh sách tất cả sản phẩm)
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _db.Products
                    .Include(p => p.Category) // Kéo theo tên danh mục cho xịn
                    .ToListAsync();

                return Ok(products); // Trả về HTTP 200 kèm danh sách JSON
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }

        // 2. GET: /api/products/{id} (Lấy chi tiết 1 sản phẩm)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _db.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return NotFound(new { message = $"Không tìm thấy sản phẩm có ID = {id}" }); // HTTP 404

                return Ok(product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }

        // 3. POST: /api/products (Tạo sản phẩm mới)
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState); // HTTP 400 nếu dữ liệu gửi lên bị thiếu/sai

                product.CreatedAt = DateTime.Now;
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                // Trả về HTTP 201 (Created) kèm đường dẫn tới sản phẩm vừa tạo
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }

        // 4. PUT: /api/products/{id} (Cập nhật sản phẩm)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                    return BadRequest(new { message = "ID trên URL và ID trong dữ liệu không khớp!" });

                // Kiểm tra xem áo có tồn tại trong database không
                var existingProduct = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (existingProduct == null)
                    return NotFound();

                _db.Entry(product).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return NoContent(); // HTTP 204: Cập nhật thành công, không cần trả về nội dung gì
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }

        // 5. DELETE: /api/products/{id} (Xóa sản phẩm)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                    return NotFound();

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                return NoContent(); // HTTP 204: Đã xóa thành công
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi máy chủ nội bộ");
            }
        }
    }
}