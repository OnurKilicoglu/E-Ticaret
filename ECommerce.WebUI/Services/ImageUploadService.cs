using ECommerce.Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.WebUI.Services;

/// <summary>
/// Image upload service implementation
/// </summary>
public class ImageUploadService : IImageUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageUploadService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 2 * 1024 * 1024; // 2MB

    public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string?> SaveImageAsync(IFormFile file, string folderName)
    {
        try
        {
            // Validate file first
            var validation = ValidateImage(file);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Invalid image file: {Errors}", string.Join(", ", validation.Errors));
                return null;
            }

            // Create upload directory
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path for database storage
            var relativePath = $"/uploads/{folderName}/{fileName}";
            _logger.LogInformation("Image saved successfully: {RelativePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image file");
            return null;
        }
    }

    public bool DeleteImage(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath)) return false;

            // Convert relative path to absolute path
            var fileName = Path.GetFileName(imagePath);
            var folderPath = Path.GetDirectoryName(imagePath)?.Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_environment.WebRootPath, folderPath?.TrimStart(Path.DirectorySeparatorChar) ?? "", fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Image deleted successfully: {ImagePath}", imagePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image file: {ImagePath}", imagePath);
            return false;
        }
    }

    public (bool IsValid, List<string> Errors) ValidateImage(IFormFile file)
    {
        var errors = new List<string>();

        if (file == null || file.Length == 0)
        {
            errors.Add("No file selected.");
            return (false, errors);
        }

        // Check file size
        if (file.Length > _maxFileSize)
        {
            errors.Add($"File size must be less than {_maxFileSize / (1024 * 1024)}MB.");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            errors.Add($"Only image files ({string.Join(", ", _allowedExtensions)}) are allowed.");
        }

        // Check content type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            errors.Add("Invalid file type. Only image files are allowed.");
        }

        return (errors.Count == 0, errors);
    }
}
