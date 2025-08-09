using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for Slider entity operations
/// </summary>
public interface ISliderService
{
    /// <summary>
    /// Get all sliders with filtering and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for title or description</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="sortBy">Sort field (title, displayOrder, createdDate)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing sliders and total count</returns>
    Task<(IEnumerable<Slider> Sliders, int TotalCount)> GetSlidersAsync(
        string? searchTerm = null,
        bool? isActive = null,
        string sortBy = "displayOrder",
        string sortOrder = "asc",
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get slider by ID
    /// </summary>
    /// <param name="id">Slider ID</param>
    /// <returns>Slider entity or null if not found</returns>
    Task<Slider?> GetSliderByIdAsync(int id);

    /// <summary>
    /// Get all active sliders ordered by display order for frontend
    /// </summary>
    /// <returns>List of active sliders</returns>
    Task<IEnumerable<Slider>> GetActiveSlidersAsync();

    /// <summary>
    /// Create new slider with image upload
    /// </summary>
    /// <param name="slider">Slider entity</param>
    /// <param name="imageFile">Image file to upload</param>
    /// <returns>Created slider or null if failed</returns>
    Task<Slider?> CreateSliderAsync(Slider slider, IFormFile? imageFile = null);

    /// <summary>
    /// Update existing slider
    /// </summary>
    /// <param name="slider">Updated slider entity</param>
    /// <param name="imageFile">New image file (optional)</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateSliderAsync(Slider slider, IFormFile? imageFile = null);

    /// <summary>
    /// Delete slider (soft delete)
    /// </summary>
    /// <param name="id">Slider ID</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteSliderAsync(int id);

    /// <summary>
    /// Hard delete slider and remove image file
    /// </summary>
    /// <param name="id">Slider ID</param>
    /// <returns>True if successful</returns>
    Task<bool> HardDeleteSliderAsync(int id);

    /// <summary>
    /// Toggle slider active status
    /// </summary>
    /// <param name="id">Slider ID</param>
    /// <returns>True if successful</returns>
    Task<bool> ToggleSliderStatusAsync(int id);

    /// <summary>
    /// Update slider display order
    /// </summary>
    /// <param name="id">Slider ID</param>
    /// <param name="newOrder">New display order</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateSliderOrderAsync(int id, int newOrder);

    /// <summary>
    /// Get next available display order
    /// </summary>
    /// <returns>Next order number</returns>
    Task<int> GetNextDisplayOrderAsync();

    /// <summary>
    /// Validate image file
    /// </summary>
    /// <param name="file">Image file to validate</param>
    /// <returns>Validation result with errors if any</returns>
    (bool IsValid, List<string> Errors) ValidateImageFile(IFormFile file);

    /// <summary>
    /// Check if slider exists
    /// </summary>
    /// <param name="id">Slider ID</param>
    /// <returns>True if exists</returns>
    Task<bool> SliderExistsAsync(int id);

    /// <summary>
    /// Get slider statistics
    /// </summary>
    /// <returns>Statistics about sliders</returns>
    Task<SliderStatistics> GetSliderStatisticsAsync();

    /// <summary>
    /// Reorder sliders - adjust display orders
    /// </summary>
    /// <param name="sliderOrders">Dictionary of slider ID and new order</param>
    /// <returns>True if successful</returns>
    Task<bool> ReorderSlidersAsync(Dictionary<int, int> sliderOrders);
}

/// <summary>
/// Slider statistics model
/// </summary>
public class SliderStatistics
{
    public int TotalSliders { get; set; }
    public int ActiveSliders { get; set; }
    public int InactiveSliders { get; set; }
    public DateTime? LastUpdated { get; set; }
    public long TotalImageSize { get; set; } // In bytes
}
