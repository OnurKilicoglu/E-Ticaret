namespace ECommerce.WebUI.Services;

/// <summary>
/// Service interface for file upload operations
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload multiple product images
    /// </summary>
    /// <param name="files">List of image files to upload</param>
    /// <param name="productId">Product ID for organizing files</param>
    /// <returns>List of uploaded file information</returns>
    Task<List<UploadedFileInfo>> UploadProductImagesAsync(List<IFormFile> files, int productId);

    /// <summary>
    /// Upload a single product image
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="productId">Product ID for organizing files</param>
    /// <returns>Uploaded file information</returns>
    Task<UploadedFileInfo> UploadProductImageAsync(IFormFile file, int productId);

    /// <summary>
    /// Delete a product image file
    /// </summary>
    /// <param name="filePath">Path to the file to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteProductImageAsync(string filePath);

    /// <summary>
    /// Validate image file
    /// </summary>
    /// <param name="file">File to validate</param>
    /// <returns>Validation result</returns>
    FileValidationResult ValidateImageFile(IFormFile file);

    /// <summary>
    /// Get optimized image path for web display
    /// </summary>
    /// <param name="originalPath">Original file path</param>
    /// <returns>Web-accessible path</returns>
    string GetWebPath(string originalPath);

    /// <summary>
    /// Create thumbnail for image
    /// </summary>
    /// <param name="originalPath">Original image path</param>
    /// <param name="thumbnailSize">Thumbnail size (default: 300x300)</param>
    /// <returns>Thumbnail path</returns>
    Task<string> CreateThumbnailAsync(string originalPath, int thumbnailSize = 300);
}

/// <summary>
/// Information about uploaded file
/// </summary>
public class UploadedFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string WebPath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// File validation result
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? ErrorMessage => Errors.Any() ? string.Join(", ", Errors) : null;
}
