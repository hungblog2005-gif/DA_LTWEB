using DA_WEB.Models;

namespace DA_WEB.Models 
{
    public class DashboardViewModel
    {
        // 1. KPI Cards
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }

        // 2. Alerts
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Order> PendingOrders { get; set; } = new List<Order>();

        // 3. Recent Orders
        public List<Order> RecentOrders { get; set; } = new List<Order>();

        // 4. Charts Data (Lưu dưới dạng chuỗi để truyền vào Javascript vẽ biểu đồ)
        public string ChartDates { get; set; } = "[]";
        public string ChartRevenues { get; set; } = "[]";
        public List<TopProductModel> TopProducts { get; set; } = new List<TopProductModel>();
    }

    // THÊM CLASS NÀY Ở DƯỚI CÙNG FILE (Bên ngoài class DashboardViewModel)
    public class TopProductModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}