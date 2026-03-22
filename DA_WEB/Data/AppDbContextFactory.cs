// File: Data/AppDbContextFactory.cs
// Tạo file này trong thư mục Data/ — chỉ dùng cho migration, không ảnh hưởng runtime

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DA_WEB.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Dùng đúng connection string trong appsettings.json của bạn
        optionsBuilder.UseSqlServer(
            "Server=MSI\\SQLEXPRESS;Database=Kizanu_Website;Trusted_Connection=True;TrustServerCertificate=True;"
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}