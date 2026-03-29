using DA_WEB.Models;

namespace DA_WEB.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Kiểm tra xem database đã có dữ liệu Category chưa. Nếu có rồi thì không nạp thêm để tránh trùng lặp.
            if (context.Categories.Any())
            {
                return;
            }

            // 1. TẠO 3 DANH MỤC WABI-SABI
            var categories = new Category[]
            {
                new Category { Name = "Áo Haori & Kimono", Description = "Phóng khoáng, tĩnh lặng và mang đậm hơi thở truyền thống Nhật Bản." },
                new Category { Name = "Quần Wide-leg", Description = "Sự thoải mái tối đa trong từng chuyển động, giải phóng cơ thể." },
                new Category { Name = "Áo Linen Tối Giản", Description = "Chất liệu tự nhiên, thô mộc, tôn vinh vẻ đẹp của sự không hoàn hảo." }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges(); // Lưu để lấy ID danh mục cho sản phẩm bên dưới

            // 2. TẠO 5 SẢN PHẨM MANG LINH HỒN KIZUNA
            var products = new Product[]
            {
                new Product {
                    Name = "Áo Haori Phong Trần (Kaze)",
                    SKU = "KZ-H001",
                    Price = 850000,
                    Stock = 50,
                    CategoryId = categories[0].Id,
                    Material = "100% Premium Linen",
                    CareInstructions = "Giặt tay nước lạnh, phơi bóng râm",
                    Description = "Chiếc áo Haori được dệt thủ công mang lại sự tĩnh lặng. Form dáng rủ nhẹ nhàng như cơn gió mùa thu (Kaze), dễ dàng phối cùng mọi trang phục thường ngày.",
                    ImageUrl = "https://images.unsplash.com/photo-1552874869-5c39ec9288dc?auto=format&fit=crop&q=80&w=600&h=800"
                },
                new Product {
                    Name = "Quần Wide-leg Xếp Ly (Aki)",
                    SKU = "KZ-P001",
                    Price = 650000,
                    SalePrice = 590000, // Đang giảm giá
                    Stock = 100,
                    CategoryId = categories[1].Id,
                    Material = "Cotton pha Linen",
                    CareInstructions = "Có thể giặt máy chế độ nhẹ",
                    Description = "Lấy cảm hứng từ trang phục võ sĩ đạo, chiếc quần ống rộng mang lại sự tự do tuyệt đối. Đường xếp ly sắc nét tạo hiệu ứng thị giác mạnh mẽ.",
                    ImageUrl = "https://images.unsplash.com/photo-1594633312681-425c7b97ccd1?auto=format&fit=crop&q=80&w=600&h=800"
                },
                new Product {
                    Name = "Áo Sơ Mi Linen Cổ Tàu (Mori)",
                    SKU = "KZ-S001",
                    Price = 450000,
                    Stock = 30,
                    CategoryId = categories[2].Id,
                    Material = "100% Organic Linen",
                    CareInstructions = "Chỉ giặt tay, không sấy nhiệt độ cao",
                    Description = "Sự tối giản thuần khiết. Bề mặt vải thô mộc tự nhiên với những hạt nếp gấp độc bản, tôn vinh triết lý Wabi-sabi.",
                    ImageUrl = "https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&q=80&w=600&h=800"
                },
                new Product {
                    Name = "Áo Khoác Noragi Linen (Yoru)",
                    SKU = "KZ-H002",
                    Price = 950000,
                    Stock = 20,
                    CategoryId = categories[0].Id,
                    Material = "Linen Wash cao cấp",
                    CareInstructions = "Giặt khô hoặc giặt tay",
                    Description = "Phiên bản hiện đại hóa của áo Noragi truyền thống. Dây buộc đắp chéo tinh tế, màu đen tuyền bí ẩn như màn đêm (Yoru).",
                    ImageUrl = "https://images.unsplash.com/photo-1578932750294-f5075e85f44a?auto=format&fit=crop&q=80&w=600&h=800"
                },
                new Product {
                    Name = "Quần Culottes Cạp Chun (Sora)",
                    SKU = "KZ-P002",
                    Price = 550000,
                    Stock = 80,
                    CategoryId = categories[1].Id,
                    Material = "Linen tơ tằm",
                    CareInstructions = "Giặt tay nước lạnh",
                    Description = "Nhẹ như mây trời. Thiết kế cạp chun thoải mái cùng cúc gỗ mộc mạc, phù hợp cho những ngày dạo phố thong dong.",
                    ImageUrl = "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?auto=format&fit=crop&q=80&w=600&h=800"
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}