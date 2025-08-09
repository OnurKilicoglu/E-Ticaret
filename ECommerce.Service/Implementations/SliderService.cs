using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for Slider entity operations
/// </summary>
public class SliderService : ISliderService
{
    private readonly ECommerceDbContext _context;
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<SliderService> _logger;

    public SliderService(
        ECommerceDbContext context,
        IImageUploadService imageUploadService,
        ILogger<SliderService> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    public async Task<(IEnumerable<Slider> Sliders, int TotalCount)> GetSlidersAsync(
        string? searchTerm = null,
        bool? isActive = null,
        string sortBy = "displayOrder",
        string sortOrder = "asc",
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.Sliders.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Title.Contains(searchTerm) || 
                                        s.Description.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(s => s.Title)
                    : query.OrderBy(s => s.Title),
                "createddate" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.CreatedDate)
                    : query.OrderBy(s => s.CreatedDate),
                "displayorder" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.DisplayOrder)
                    : query.OrderBy(s => s.DisplayOrder),
                _ => query.OrderBy(s => s.DisplayOrder)
            };

            // Apply pagination
            var sliders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sliders, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sliders with filters");
            return (Enumerable.Empty<Slider>(), 0);
        }
    }

    public async Task<Slider?> GetSliderByIdAsync(int id)
    {
        try
        {
            return await _context.Sliders
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slider by ID: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<Slider>> GetActiveSlidersAsync()
    {
        try
        {
            return await _context.Sliders
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sliders");
            return Enumerable.Empty<Slider>();
        }
    }

    public async Task<Slider?> CreateSliderAsync(Slider slider, IFormFile? imageFile = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Handle image upload
            if (imageFile != null)
            {
                var imagePath = await _imageUploadService.SaveImageAsync(imageFile, "slider");
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogError("Failed to save image file for slider");
                    return null;
                }

                slider.ImageUrl = imagePath;
            }

            // Set display order if not provided
            if (slider.DisplayOrder == 0)
            {
                slider.DisplayOrder = await GetNextDisplayOrderAsync();
            }

            // Set created date
            slider.CreatedDate = DateTime.UtcNow;

            _context.Sliders.Add(slider);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Created slider with ID: {Id}", slider.Id);
            return slider;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating slider");

            // Clean up uploaded file if it exists
            if (!string.IsNullOrEmpty(slider.ImageUrl))
            {
                _imageUploadService.DeleteImage(slider.ImageUrl);
            }

            return null;
        }
    }

    public async Task<bool> UpdateSliderAsync(Slider slider, IFormFile? imageFile = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingSlider = await _context.Sliders.FindAsync(slider.Id);
            if (existingSlider == null)
            {
                _logger.LogWarning("Slider not found for update: {Id}", slider.Id);
                return false;
            }

            var oldImagePath = existingSlider.ImageUrl;

            // Handle new image upload
            if (imageFile != null)
            {
                var imagePath = await _imageUploadService.SaveImageAsync(imageFile, "slider");
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogError("Failed to save new image file for slider");
                    return false;
                }

                slider.ImageUrl = imagePath;
            }
            else
            {
                // Keep existing image
                slider.ImageUrl = existingSlider.ImageUrl;
            }

            // Update properties
            existingSlider.Title = slider.Title;
            existingSlider.Description = slider.Description;
            existingSlider.Link = slider.Link;
            existingSlider.DisplayOrder = slider.DisplayOrder;
            existingSlider.IsActive = slider.IsActive;
            existingSlider.ImageUrl = slider.ImageUrl;
            existingSlider.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Delete old image if new one was uploaded
            if (imageFile != null && !string.IsNullOrEmpty(oldImagePath))
            {
                _imageUploadService.DeleteImage(oldImagePath);
            }

            _logger.LogInformation("Updated slider with ID: {Id}", slider.Id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating slider with ID: {Id}", slider.Id);

            // Clean up new image if upload failed
            if (imageFile != null && !string.IsNullOrEmpty(slider.ImageUrl))
            {
                _imageUploadService.DeleteImage(slider.ImageUrl);
            }

            return false;
        }
    }

    public async Task<bool> DeleteSliderAsync(int id)
    {
        try
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                _logger.LogWarning("Slider not found for soft delete: {Id}", id);
                return false;
            }

            slider.IsActive = false;
            slider.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted slider with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting slider with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> HardDeleteSliderAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                _logger.LogWarning("Slider not found for hard delete: {Id}", id);
                return false;
            }

            var imagePath = slider.ImageUrl;

            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Delete image file
            if (!string.IsNullOrEmpty(imagePath))
            {
                _imageUploadService.DeleteImage(imagePath);
            }

            _logger.LogInformation("Hard deleted slider with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error hard deleting slider with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ToggleSliderStatusAsync(int id)
    {
        try
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                _logger.LogWarning("Slider not found for status toggle: {Id}", id);
                return false;
            }

            slider.IsActive = !slider.IsActive;
            slider.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled slider status for ID: {Id} to {Status}", id, slider.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling slider status for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateSliderOrderAsync(int id, int newOrder)
    {
        try
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                _logger.LogWarning("Slider not found for order update: {Id}", id);
                return false;
            }

            slider.DisplayOrder = newOrder;
            slider.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated slider order for ID: {Id} to {Order}", id, newOrder);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating slider order for ID: {Id}", id);
            return false;
        }
    }

    public async Task<int> GetNextDisplayOrderAsync()
    {
        try
        {
            var maxOrder = await _context.Sliders
                .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;
            return maxOrder + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next display order");
            return 1;
        }
    }

    public (bool IsValid, List<string> Errors) ValidateImageFile(IFormFile file)
    {
        return _imageUploadService.ValidateImage(file);
    }

    public async Task<bool> SliderExistsAsync(int id)
    {
        try
        {
            return await _context.Sliders.AnyAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if slider exists: {Id}", id);
            return false;
        }
    }

    public async Task<SliderStatistics> GetSliderStatisticsAsync()
    {
        try
        {
            var totalSliders = await _context.Sliders.CountAsync();
            var activeSliders = await _context.Sliders.CountAsync(s => s.IsActive);
            var lastUpdated = await _context.Sliders
                .Where(s => s.UpdatedDate.HasValue)
                .MaxAsync(s => (DateTime?)s.UpdatedDate);

            // Note: Total image size calculation would require file system access
            // which is handled by the ImageUploadService in the WebUI layer
            long totalSize = 0;

            return new SliderStatistics
            {
                TotalSliders = totalSliders,
                ActiveSliders = activeSliders,
                InactiveSliders = totalSliders - activeSliders,
                LastUpdated = lastUpdated,
                TotalImageSize = totalSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slider statistics");
            return new SliderStatistics();
        }
    }

    public async Task<bool> ReorderSlidersAsync(Dictionary<int, int> sliderOrders)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var kvp in sliderOrders)
            {
                var slider = await _context.Sliders.FindAsync(kvp.Key);
                if (slider != null)
                {
                    slider.DisplayOrder = kvp.Value;
                    slider.UpdatedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Reordered {Count} sliders", sliderOrders.Count);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error reordering sliders");
            return false;
        }
    }


}
