using Microsoft.EntityFrameworkCore;
using ASP_MVC_HW02;

namespace ASP_MVC_HW02;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
}
