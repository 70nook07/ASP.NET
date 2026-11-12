using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ASP_MVC_HW02;

public class HomeController(ShopDbContext db, IImageService images) : Controller
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    [HttpGet("/")]
    public IActionResult Index() =>
        View(db.Products.Include(p => p.Category).OrderBy(p => p.Id).ToList());

    [HttpGet("details/{id:int}")]
    public IActionResult Details(int id)
    {
        var p = db.Products.Include(x => x.Category).FirstOrDefault(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    [HttpGet("create")]
    public IActionResult Create() => ProductForm(new ProductViewModel());

    [HttpPost("create")]
    public IActionResult Create(ProductViewModel model)
    {
        ValidateProduct(model, imageRequired: true);
        if (!ModelState.IsValid) return ProductForm(model);

        db.Products.Add(new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CategoryId = model.CategoryId,
            ImagePath = images.Save(model.Image!)
        });
        db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public IActionResult Edit(int id)
    {
        var p = db.Products.Find(id);
        if (p == null) return NotFound();

        return ProductForm(new ProductViewModel
        {
            Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price, CategoryId = p.CategoryId
        });
    }

    [HttpPost("edit/{id:int}")]
    public IActionResult Edit(int id, ProductViewModel model)
    {
        var p = db.Products.Find(id);
        if (p == null) return NotFound();

        model.Id = id;
        ValidateProduct(model, imageRequired: false);
        if (!ModelState.IsValid) return ProductForm(model);

        var oldImage = p.ImagePath;
        if (model.Image != null) p.ImagePath = images.Save(model.Image);
        p.Name = model.Name;
        p.Description = model.Description;
        p.Price = model.Price;
        p.CategoryId = model.CategoryId;
        db.SaveChanges();

        if (p.ImagePath != oldImage) images.Delete(oldImage);   // only after the DB update succeeded
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("delete/{id:int}")]
    public IActionResult DeleteConfirmation(int id)
    {
        var p = db.Products.Include(x => x.Category).FirstOrDefault(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    [HttpPost("delete/{id:int}")]
    public IActionResult Delete(int id)
    {
        var p = db.Products.Find(id);
        if (p == null) return NotFound();

        db.Products.Remove(p);
        db.SaveChanges();
        images.Delete(p.ImagePath);
        return RedirectToAction(nameof(Index));
    }

    private IActionResult ProductForm(ProductViewModel model)
    {
        ViewBag.Categories = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name");
        return View("ProductForm", model);
    }

    // Checks that the annotations can't: the category exists and the upload is an image
    private void ValidateProduct(ProductViewModel model, bool imageRequired)
    {
        if (!db.Categories.Any(c => c.Id == model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "Please choose a category.");

        if (model.Image == null)
        {
            if (imageRequired) ModelState.AddModelError(nameof(model.Image), "Please choose an image.");
        }
        else if (!ImageExtensions.Contains(Path.GetExtension(model.Image.FileName).ToLowerInvariant()))
        {
            ModelState.AddModelError(nameof(model.Image), "Only .jpg, .png, .gif and .webp images are allowed.");
        }
    }
}
