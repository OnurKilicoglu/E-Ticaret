using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ECommerce.WebUI.Services;

/// <summary>
/// Service implementation for file upload operations
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB
    private const string UploadPath = "uploads/products";

    public FileUploadService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<List<UploadedFileInfo>> UploadProductImagesAsync(List<IFormFile> files, int productId)
    {
        var uploadedFiles = new List<UploadedFileInfo>();

        foreach (var file in files)
        {
            var uploadedFile = await UploadProductImageAsync(file, productId);
            if (!string.IsNullOrEmpty(uploadedFile.FilePath))
            {
                uploadedFiles.Add(uploadedFile);
            }
        }

        return uploadedFiles;
    }

    public async Task<UploadedFileInfo> UploadProductImageAsync(IFormFile file, int productId)
    {
        var uploadedFile = new UploadedFileInfo();

        // Validate file
        var validation = ValidateImageFile(file);
        if (!validation.IsValid)
        {
            return uploadedFile;
        }

        try
        {
            // Create directory if it doesn't exist
            var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, UploadPath);
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Create product-specific directory
            var productDirectory = Path.Combine(uploadDirectory, productId.ToString());
            if (!Directory.Exists(productDirectory))
            {
                Directory.CreateDirectory(productDirectory);
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(productDirectory, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Create thumbnail
            var thumbnailPath = await CreateThumbnailAsync(filePath);

            // Set upload info
            uploadedFile.FileName = file.FileName;
            uploadedFile.FilePath = filePath;
            uploadedFile.WebPath = GetWebPath(filePath);
            uploadedFile.ThumbnailPath = GetWebPath(thumbnailPath);
            uploadedFile.FileSize = file.Length;
            uploadedFile.ContentType = file.ContentType;
        }
        catch (Exception ex)
        {
            // Log error (implement logging as needed)
            Console.WriteLine($"File upload error: {ex.Message}");
        }

        return uploadedFile;
    }

    public async Task<bool> DeleteProductImageAsync(string filePath)
    {
        try
        {
            // Convert web path to physical path if needed
            var physicalPath = filePath;
            if (filePath.StartsWith("/") || filePath.StartsWith("\\"))
            {
                physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/', '\\'));
            }

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);

                // Also delete thumbnail if exists
                var thumbnailPath = GetThumbnailPath(physicalPath);
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"File deletion error: {ex.Message}");
        }

        return false;
    }

    public FileValidationResult ValidateImageFile(IFormFile file)
    {
        var result = new FileValidationResult { IsValid = true };

        if (file == null || file.Length == 0)
        {
            result.IsValid = false;
            result.Errors.Add("No file selected");
            return result;
        }

        // Check file size
        if (file.Length > MaxFileSize)
        {
            result.IsValid = false;
            result.Errors.Add($"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)}MB");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            result.IsValid = false;
            result.Errors.Add($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }

        // Check MIME type
        if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            result.IsValid = false;
            result.Errors.Add($"File MIME type '{file.ContentType}' is not allowed");
        }

        // Additional security check - verify file signature
        if (result.IsValid && !IsValidImageFile(file))
        {
            result.IsValid = false;
            result.Errors.Add("File does not appear to be a valid image");
        }

        return result;
    }

    public string GetWebPath(string physicalPath)
    {
        if (string.IsNullOrEmpty(physicalPath))
            return string.Empty;

        var webRootPath = _webHostEnvironment.WebRootPath;
        if (physicalPath.StartsWith(webRootPath))
        {
            return physicalPath.Substring(webRootPath.Length).Replace('\\', '/');
        }

        return physicalPath.Replace('\\', '/');
    }

    public async Task<string> CreateThumbnailAsync(string originalPath, int thumbnailSize = 300)
    {
        try
        {
            var directory = Path.GetDirectoryName(originalPath);
            var filename = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);
            var thumbnailPath = Path.Combine(directory!, $"{filename}_thumb_{thumbnailSize}{extension}");

            using var image = await Image.LoadAsync(originalPath);
            
            // Calculate resize dimensions maintaining aspect ratio
            var ratio = Math.Min((double)thumbnailSize / image.Width, (double)thumbnailSize / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));
            await image.SaveAsync(thumbnailPath);

            return thumbnailPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thumbnail creation error: {ex.Message}");
            return originalPath; // Return original if thumbnail creation fails
        }
    }

    private bool IsValidImageFile(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            // Check for common image file signatures
            // JPEG: FF D8 FF
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                return true;

            // GIF: 47 49 46 38 (GIF8)
            if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                return true;

            // WebP: 52 49 46 46 (RIFF) + WebP signature
            if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
            {
                stream.Seek(8, SeekOrigin.Begin);
                var webpBuffer = new byte[4];
                stream.Read(webpBuffer, 0, 4);
                if (webpBuffer[0] == 0x57 && webpBuffer[1] == 0x45 && webpBuffer[2] == 0x42 && webpBuffer[3] == 0x50)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private string GetThumbnailPath(string originalPath)
    {
        var directory = Path.GetDirectoryName(originalPath);
        var filename = Path.GetFileNameWithoutExtension(originalPath);
        var extension = Path.GetExtension(originalPath);
        return Path.Combine(directory!, $"{filename}_thumb_300{extension}");
    }
}
