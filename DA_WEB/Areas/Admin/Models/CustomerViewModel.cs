namespace DA_WEB.Areas.Admin.Models
{
    public class CustomerViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalOrders { get; set; } // Tổng số đơn hàng đã đặt
        public decimal TotalSpent { get; set; } // Tổng số tiền đã mua
        public bool IsLocked { get; set; } // <--- Thêm dòng này
    }
}