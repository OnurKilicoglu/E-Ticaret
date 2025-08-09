using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for Slider management in Admin area
/// </summary>
[Area("Admin")]
public class SliderController : Controller
{
    private readonly ISliderService _sliderService;

    public SliderController(ISliderService sliderService)
    {
        _sliderService = sliderService;
    }

    /// <summary>
    /// Display list of sliders with search, filtering, and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, bool? isActive,
        string sortBy = "displayOrder", string sortOrder = "asc",
        int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Slider Management";

        try
        {
            var (sliders, totalCount) = await _sliderService.GetSlidersAsync(
                searchTerm, isActive, sortBy, sortOrder, page, pageSize);

            var statistics = await _sliderService.GetSliderStatisticsAsync();

            var viewModel = new SliderListViewModel
            {
                Sliders = sliders.Select(s => new SliderItemViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    ImagePath = s.ImageUrl,
                    LinkUrl = s.Link,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                IsActive = isActive,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Statistics = new SliderStatisticsViewModel
                {
                    TotalSliders = statistics.TotalSliders,
                    ActiveSliders = statistics.ActiveSliders,
                    InactiveSliders = statistics.InactiveSliders,
                    LastUpdated = statistics.LastUpdated,
                    TotalImageSizeMB = Math.Round(statistics.TotalImageSize / (1024.0 * 1024.0), 2)
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading sliders: {ex.Message}";
            return View(new SliderListViewModel());
        }
    }

    /// <summary>
    /// Display detailed slider information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Slider Details";

        try
        {
            var slider = await _sliderService.GetSliderByIdAsync(id);
            if (slider == null)
            {
                TempData["ErrorMessage"] = "Slider not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new SliderDetailViewModel
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description,
                ImagePath = slider.ImageUrl,
                LinkUrl = slider.Link,
                DisplayOrder = slider.DisplayOrder,
                IsActive = slider.IsActive,
                CreatedDate = slider.CreatedDate,
                UpdatedDate = slider.UpdatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading slider details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display slider creation form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create New Slider";

        var viewModel = new SliderCreateViewModel
        {
            DisplayOrder = await _sliderService.GetNextDisplayOrderAsync(),
            IsActive = true
        };

        return View(viewModel);
    }

    /// <summary>
    /// Process slider creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SliderCreateViewModel model)
    {
        ViewData["Title"] = "Create New Slider";

        if (ModelState.IsValid)
        {
            try
            {
                // Validate image file if provided
                if (model.ImageFile != null)
                {
                    var validation = _sliderService.ValidateImageFile(model.ImageFile);
                    if (!validation.IsValid)
                    {
                        foreach (var error in validation.Errors)
                        {
                            ModelState.AddModelError("ImageFile", error);
                        }
                        return View(model);
                    }
                }
                else if (string.IsNullOrEmpty(model.ImagePath))
                {
                    ModelState.AddModelError("ImageFile", "Please select an image file.");
                    return View(model);
                }

                var slider = new Slider
                {
                    Title = model.Title,
                    Description = model.Description,
                    Link = model.LinkUrl,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                var createdSlider = await _sliderService.CreateSliderAsync(slider, model.ImageFile);

                if (createdSlider != null)
                {
                    TempData["SuccessMessage"] = $"Slider '{model.Title}' has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = createdSlider.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create slider.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating slider: {ex.Message}";
            }
        }

        // Reset display order if model is invalid
        if (model.DisplayOrder == 0)
        {
            model.DisplayOrder = await _sliderService.GetNextDisplayOrderAsync();
        }

        return View(model);
    }

    /// <summary>
    /// Display slider edit form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Slider";

        try
        {
            var slider = await _sliderService.GetSliderByIdAsync(id);
            if (slider == null)
            {
                TempData["ErrorMessage"] = "Slider not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new SliderEditViewModel
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description,
                ImagePath = slider.ImageUrl,
                LinkUrl = slider.Link,
                DisplayOrder = slider.DisplayOrder,
                IsActive = slider.IsActive,
                CreatedDate = slider.CreatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading slider: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process slider update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SliderEditViewModel model)
    {
        ViewData["Title"] = "Edit Slider";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid slider ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Validate new image file if provided
                if (model.ImageFile != null)
                {
                    var validation = _sliderService.ValidateImageFile(model.ImageFile);
                    if (!validation.IsValid)
                    {
                        foreach (var error in validation.Errors)
                        {
                            ModelState.AddModelError("ImageFile", error);
                        }
                        return View(model);
                    }
                }

                var slider = new Slider
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Link = model.LinkUrl,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                var success = await _sliderService.UpdateSliderAsync(slider, model.ImageFile);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Slider '{model.Title}' has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update slider.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating slider: {ex.Message}";
            }
        }

        return View(model);
    }

    /// <summary>
    /// Display slider deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Slider";

        try
        {
            var slider = await _sliderService.GetSliderByIdAsync(id);
            if (slider == null)
            {
                TempData["ErrorMessage"] = "Slider not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new SliderDeleteViewModel
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description,
                ImagePath = slider.ImageUrl,
                LinkUrl = slider.Link,
                DisplayOrder = slider.DisplayOrder,
                IsActive = slider.IsActive,
                CreatedDate = slider.CreatedDate,
                CanBeHardDeleted = true // Can be made configurable based on business rules
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading slider: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process slider deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, SliderDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                bool success;
                
                if (model.HardDelete && model.CanBeHardDeleted)
                {
                    success = await _sliderService.HardDeleteSliderAsync(id);
                    TempData["SuccessMessage"] = $"Slider '{model.Title}' has been permanently deleted.";
                }
                else
                {
                    success = await _sliderService.DeleteSliderAsync(id);
                    TempData["SuccessMessage"] = $"Slider '{model.Title}' has been deactivated.";
                }

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete slider.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting slider: {ex.Message}";
            return View("Delete", model);
        }
    }

    /// <summary>
    /// AJAX endpoint for quick status toggle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            var success = await _sliderService.ToggleSliderStatusAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Slider status updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update slider status" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for updating display order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateOrder(int id, int newOrder)
    {
        try
        {
            var success = await _sliderService.UpdateSliderOrderAsync(id, newOrder);
            
            if (success)
            {
                return Json(new { success = true, message = "Display order updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update display order" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for batch reordering sliders
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReorderSliders([FromBody] Dictionary<int, int> sliderOrders)
    {
        try
        {
            var success = await _sliderService.ReorderSlidersAsync(sliderOrders);
            
            if (success)
            {
                return Json(new { success = true, message = "Sliders reordered successfully" });
            }
            
            return Json(new { success = false, message = "Failed to reorder sliders" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}


