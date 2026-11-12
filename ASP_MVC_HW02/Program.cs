using ASP_MVC_HW02;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Validate the anti-forgery token
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDbContext<ShopDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=shop.db"));

builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();

app.UseRequestLocalization("en-US");
app.UseHttpsRedirection();
app.UseStaticFiles();   // wwwroot: css + uploaded images
app.MapControllers();

// First run seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    db.Database.EnsureCreated();

    if (!db.Categories.Any())
    {
        Category electronics = new() { Name = "Electronics" }, clothing = new() { Name = "Clothing" };
        db.Categories.AddRange(electronics, clothing, new Category { Name = "Accessories" });
        db.Products.AddRange(
            new Product { Name = "Headphones", Description = "Wireless headphones.", Price = 79.99m, Category = electronics, ImagePath = ImageService.DefaultImage },
            new Product { Name = "T-Shirt", Description = "Cotton t-shirt.", Price = 25.00m, Category = clothing, ImagePath = ImageService.DefaultImage });
        db.SaveChanges();
    }
}

app.Run();
