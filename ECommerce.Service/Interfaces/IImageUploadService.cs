using Microsoft.AspNetCore.Http;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for image upload operations
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Save uploaded image file
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="folderName">Target folder name (e.g., "slider", "product")</param>
    /// <returns>Relative path to saved image or null if failed</returns>
    Task<string?> SaveImageAsync(IFormFile file, string folderName);

    /// <summary>
    /// Delete image file
    /// </summary>
    /// <param name="imagePath">Relative path to image</param>
    /// <returns>True if successful</returns>
    bool DeleteImage(string imagePath);

    /// <summary>
    /// Validate image file
    /// </summary>
    /// <param name="file">Image file to validate</param>
    /// <returns>Validation result with errors if any</returns>
    (bool IsValid, List<string> Errors) ValidateImage(IFormFile file);
}
