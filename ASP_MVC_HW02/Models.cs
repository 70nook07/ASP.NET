using System.ComponentModel.DataAnnotations;

namespace ASP_MVC_HW02;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = "";
    [MaxLength(500)] public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string ImagePath { get; set; } = "";
    public Category Category { get; set; } = null!;
}

// Add / Edit form. Id == 0 = "new product".
public class ProductViewModel
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(500)] public string Description { get; set; } = "";
    [Range(0.01, 1_000_000)] public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; }
}
