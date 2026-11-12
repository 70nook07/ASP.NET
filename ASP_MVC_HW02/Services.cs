namespace ASP_MVC_HW02;

public interface IImageService
{
    string Save(IFormFile file);
    void Delete(string path);
}

public class ImageService(IWebHostEnvironment env) : IImageService
{
    public const string DefaultImage = "/images/default.svg";

    private string Folder => Path.Combine(env.WebRootPath, "images");

    // Saves the upload under a guid name
    public string Save(IFormFile file)
    {
        Directory.CreateDirectory(Folder);
        var name = Guid.NewGuid() + Path.GetExtension(file.FileName).ToLowerInvariant();
        using var stream = File.Create(Path.Combine(Folder, name));
        file.CopyTo(stream);
        return $"/images/{name}";
    }

    public void Delete(string path)
    {
        if (path == DefaultImage) return;
        var full = Path.Combine(env.WebRootPath, path.TrimStart('/'));
        if (File.Exists(full)) File.Delete(full);
    }
}
