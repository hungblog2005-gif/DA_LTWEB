using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DA_WEB.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }

        // Ảnh đại diện chính (Thumbnail)
        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // DANH SÁCH ẢNH PHỤ (Thêm dòng này)
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public string? Material { get; set; } // Ví dụ: 100% Linen
        public string? CareInstructions { get; set; } // Ví dụ: Hand wash only
        public string? SKU { get; set; } // Ví dụ: KZ-0003

        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}