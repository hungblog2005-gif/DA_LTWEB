// File: Models/Order.cs

using System.ComponentModel.DataAnnotations;

namespace DA_WEB.Models
{
    public class Order
    {
        public int Id { get; set; }

        // ← THÊM [BindNever] hoặc bỏ [Required]
        // UserId được set trong Controller, không phải từ form
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        public string Address { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Items không bind từ form → không validate
        public List<OrderItem> Items { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán.")]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? StripeSessionId { get; set; }
    }
}