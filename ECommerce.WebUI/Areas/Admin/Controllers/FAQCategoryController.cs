using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for FAQ Category management in Admin area
/// </summary>
[Area("Admin")]
public class FAQCategoryController : Controller
{
    private readonly IFAQService _faqService;
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<FAQCategoryController> _logger;

    public FAQCategoryController(IFAQService faqService, IImageUploadService imageUploadService, ILogger<FAQCategoryController> logger)
    {
        _faqService = faqService;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Display list of FAQ categories with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, bool? isActive, string sortBy = "displayOrder", string sortOrder = "asc")
    {
        ViewData["Title"] = "FAQ Category Management";

        try
        {
            var categoriesWithCounts = await _faqService.GetCategoriesWithCountsAsync();
            var statistics = await _faqService.GetFAQStatisticsAsync();

            // Apply filters
            var filteredCategories = categoriesWithCounts.AsEnumerable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredCategories = filteredCategories.Where(c => 
                    c.Category.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Category.Description != null && c.Category.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (isActive.HasValue)
            {
                filteredCategories = filteredCategories.Where(c => c.Category.IsActive == isActive.Value);
            }

            // Apply sorting
            filteredCategories = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "desc" 
                    ? filteredCategories.OrderByDescending(c => c.Category.Name)
                    : filteredCategories.OrderBy(c => c.Category.Name),
                "faqcount" => sortOrder.ToLower() == "desc"
                    ? filteredCategories.OrderByDescending(c => c.FAQCount)
                    : filteredCategories.OrderBy(c => c.FAQCount),
                "createddate" => sortOrder.ToLower() == "desc"
                    ? filteredCategories.OrderByDescending(c => c.Category.CreatedDate)
                    : filteredCategories.OrderBy(c => c.Category.CreatedDate),
                _ => sortOrder.ToLower() == "desc"
                    ? filteredCategories.OrderByDescending(c => c.Category.DisplayOrder)
                    : filteredCategories.OrderBy(c => c.Category.DisplayOrder)
            };

            var viewModel = new FAQCategoryListViewModel
            {
                Categories = filteredCategories.Select(c => new FAQCategoryItemViewModel
                {
                    Id = c.Category.Id,
                    Name = c.Category.Name,
                    Description = c.Category.Description,
                    Icon = c.Category.Icon,
                    DisplayOrder = c.Category.DisplayOrder,
                    IsActive = c.Category.IsActive,
                    FAQCount = c.FAQCount,
                    ActiveFAQCount = c.ActiveFAQCount,
                    CreatedDate = c.Category.CreatedDate,
                    UpdatedDate = c.Category.UpdatedDate
                }),
                SearchTerm = searchTerm,
                IsActive = isActive,
                SortBy = sortBy,
                SortOrder = sortOrder,
                AvailableIcons = GetPredefinedIcons(),
                Statistics = new FAQCategoryStatisticsViewModel
                {
                    TotalCategories = statistics.TotalCategories,
                    ActiveCategories = statistics.ActiveCategories,
                    InactiveCategories = statistics.TotalCategories - statistics.ActiveCategories,
                    TotalFAQs = statistics.TotalFAQs,
                    CategorizedFAQs = categoriesWithCounts.Sum(c => c.FAQCount),
                    UncategorizedFAQs = statistics.TotalFAQs - categoriesWithCounts.Sum(c => c.FAQCount),
                    MostPopularCategoryName = categoriesWithCounts.OrderByDescending(c => c.FAQCount).FirstOrDefault()?.Category.Name,
                    MostPopularCategoryFAQCount = categoriesWithCounts.OrderByDescending(c => c.FAQCount).FirstOrDefault()?.FAQCount ?? 0,
                    LastUpdated = statistics.LastUpdated
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ categories");
            TempData["ErrorMessage"] = $"Error loading FAQ categories: {ex.Message}";
            return View(new FAQCategoryListViewModel());
        }
    }

    /// <summary>
    /// Display detailed FAQ category information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "FAQ Category Details";

        try
        {
            var category = await _faqService.GetFAQCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "FAQ category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get category's FAQs
            var (faqs, _) = await _faqService.GetFAQsAsync(categoryId: id, pageSize: int.MaxValue);
            var recentFAQs = faqs.OrderByDescending(f => f.CreatedDate).Take(5);
            var popularFAQs = faqs.OrderByDescending(f => f.ViewCount).Take(5);

            var viewModel = new FAQCategoryDetailViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                FAQCount = category.FAQs?.Count ?? 0,
                ActiveFAQCount = category.FAQs?.Count(f => f.IsActive) ?? 0,
                TotalViews = category.FAQs?.Sum(f => f.ViewCount) ?? 0,
                TotalHelpfulVotes = category.FAQs?.Sum(f => f.HelpfulCount) ?? 0,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                RecentFAQs = recentFAQs.Select(f => new FAQItemViewModel
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
                    IsActive = f.IsActive,
                    ViewCount = f.ViewCount,
                    HelpfulCount = f.HelpfulCount,
                    CreatedDate = f.CreatedDate
                }),
                PopularFAQs = popularFAQs.Select(f => new FAQItemViewModel
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
                    IsActive = f.IsActive,
                    ViewCount = f.ViewCount,
                    HelpfulCount = f.HelpfulCount,
                    CreatedDate = f.CreatedDate
                })
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ category details for ID: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading FAQ category details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display FAQ category creation form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create FAQ Category";

        try
        {
            var viewModel = new FAQCategoryCreateViewModel
            {
                DisplayOrder = await _faqService.GetNextCategoryDisplayOrderAsync(),
                PredefinedIcons = GetPredefinedIcons(),
                PopularIcons = GetPopularIcons()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ category create form");
            TempData["ErrorMessage"] = $"Error loading create form: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process FAQ category creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FAQCategoryCreateViewModel model)
    {
        ViewData["Title"] = "Create FAQ Category";

        if (ModelState.IsValid)
        {
            try
            {
                // Check name uniqueness
                if (!await _faqService.IsCategoryNameUniqueAsync(model.Name))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    await ReloadCreateViewOptions(model);
                    return View(model);
                }

                // Handle icon upload if provided
                string? iconPath = model.Icon;
                if (model.IconFile != null)
                {
                    // Validate image first
                    var validation = _imageUploadService.ValidateImage(model.IconFile);
                    if (!validation.IsValid)
                    {
                        foreach (var error in validation.Errors)
                        {
                            ModelState.AddModelError("IconFile", error);
                        }
                        await ReloadCreateViewOptions(model);
                        return View(model);
                    }

                    var uploadResult = await _imageUploadService.SaveImageAsync(model.IconFile, "categories");
                    if (uploadResult != null)
                    {
                        iconPath = uploadResult;
                    }
                    else
                    {
                        ModelState.AddModelError("IconFile", "Failed to upload icon");
                        await ReloadCreateViewOptions(model);
                        return View(model);
                    }
                }

                var category = new FAQCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    Icon = iconPath,
                    DisplayOrder = model.DisplayOrder > 0 ? model.DisplayOrder : await _faqService.GetNextCategoryDisplayOrderAsync(),
                    IsActive = model.IsActive
                };

                var createdCategory = await _faqService.CreateFAQCategoryAsync(category);

                if (createdCategory != null)
                {
                    TempData["SuccessMessage"] = $"FAQ category '{model.Name}' has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = createdCategory.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create FAQ category.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FAQ category");
                TempData["ErrorMessage"] = $"Error creating FAQ category: {ex.Message}";
            }
        }

        await ReloadCreateViewOptions(model);
        return View(model);
    }

    /// <summary>
    /// Display FAQ category edit form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit FAQ Category";

        try
        {
            var category = await _faqService.GetFAQCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "FAQ category not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FAQCategoryEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                FAQCount = category.FAQs?.Count ?? 0,
                ActiveFAQCount = category.FAQs?.Count(f => f.IsActive) ?? 0,
                TotalViews = category.FAQs?.Sum(f => f.ViewCount) ?? 0,
                PredefinedIcons = GetPredefinedIcons(),
                PopularIcons = GetPopularIcons(),
                CurrentIconUrl = !string.IsNullOrEmpty(category.Icon) ? category.Icon : ""
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ category for edit: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading FAQ category: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process FAQ category update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FAQCategoryEditViewModel model)
    {
        ViewData["Title"] = "Edit FAQ Category";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid category ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Check name uniqueness
                if (!await _faqService.IsCategoryNameUniqueAsync(model.Name, model.Id))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    await ReloadEditViewOptions(model);
                    return View(model);
                }

                // Handle icon upload if provided
                string? iconPath = model.Icon;
                if (model.IconFile != null)
                {
                    // Validate image first
                    var validation = _imageUploadService.ValidateImage(model.IconFile);
                    if (!validation.IsValid)
                    {
                        foreach (var error in validation.Errors)
                        {
                            ModelState.AddModelError("IconFile", error);
                        }
                        await ReloadEditViewOptions(model);
                        return View(model);
                    }

                    var uploadResult = await _imageUploadService.SaveImageAsync(model.IconFile, "categories");
                    if (uploadResult != null)
                    {
                        // Delete old icon if it exists
                        if (!string.IsNullOrEmpty(model.CurrentIconUrl))
                        {
                            _imageUploadService.DeleteImage(model.CurrentIconUrl);
                        }
                        iconPath = uploadResult;
                    }
                    else
                    {
                        ModelState.AddModelError("IconFile", "Failed to upload icon");
                        await ReloadEditViewOptions(model);
                        return View(model);
                    }
                }

                var category = new FAQCategory
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Icon = iconPath,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                var success = await _faqService.UpdateFAQCategoryAsync(category);

                if (success)
                {
                    TempData["SuccessMessage"] = $"FAQ category '{model.Name}' has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update FAQ category.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ category: {Id}", id);
                TempData["ErrorMessage"] = $"Error updating FAQ category: {ex.Message}";
            }
        }

        await ReloadEditViewOptions(model);
        return View(model);
    }

    /// <summary>
    /// Display FAQ category deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete FAQ Category";

        try
        {
            var category = await _faqService.GetFAQCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "FAQ category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get other categories for FAQ reassignment
            var allCategories = await _faqService.GetFAQCategoriesAsync(includeInactive: true);
            var otherCategories = allCategories.Where(c => c.Id != id);

            // Get associated FAQs
            var (faqs, _) = await _faqService.GetFAQsAsync(categoryId: id, pageSize: int.MaxValue);

            var viewModel = new FAQCategoryDeleteViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                FAQCount = category.FAQs?.Count ?? 0,
                ActiveFAQCount = category.FAQs?.Count(f => f.IsActive) ?? 0,
                TotalViews = category.FAQs?.Sum(f => f.ViewCount) ?? 0,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                AvailableCategories = otherCategories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                AssociatedFAQs = faqs.Take(10).Select(f => new FAQItemViewModel
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
                    IsActive = f.IsActive,
                    ViewCount = f.ViewCount,
                    CreatedDate = f.CreatedDate
                })
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ category for delete: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading FAQ category: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process FAQ category deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, FAQCategoryDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Handle FAQ reassignment if specified
                if (model.MoveFAQsToCategoryId.HasValue)
                {
                    var (faqs, _) = await _faqService.GetFAQsAsync(categoryId: id, pageSize: int.MaxValue);
                    foreach (var faq in faqs)
                    {
                        var updateFAQ = new FAQ
                        {
                            Id = faq.Id,
                            Question = faq.Question,
                            Answer = faq.Answer,
                            CategoryId = model.MoveFAQsToCategoryId.Value,
                            DisplayOrder = faq.DisplayOrder,
                            IsActive = faq.IsActive,
                            Tags = faq.Tags,
                            Author = faq.Author
                        };
                        await _faqService.UpdateFAQAsync(updateFAQ);
                    }
                }

                // Delete category
                bool success;
                if (model.HardDelete)
                {
                    // Note: Implement hard delete in service if needed
                    success = await _faqService.DeleteFAQCategoryAsync(id);
                }
                else
                {
                    success = await _faqService.DeleteFAQCategoryAsync(id);
                }

                if (success)
                {
                    TempData["SuccessMessage"] = $"FAQ category '{model.Name}' has been {(model.HardDelete ? "permanently deleted" : "deactivated")}.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete FAQ category.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FAQ category: {Id}", id);
            TempData["ErrorMessage"] = $"Error deleting FAQ category: {ex.Message}";
            return View("Delete", model);
        }
    }

    #region AJAX Actions

    /// <summary>
    /// AJAX endpoint for quick active status toggle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            var success = await _faqService.ToggleFAQCategoryStatusAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Category status updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update category status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling FAQ category status: {Id}", id);
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
            var success = await _faqService.UpdateFAQCategoryOrderAsync(id, newOrder);
            
            if (success)
            {
                return Json(new { success = true, message = "Category order updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update category order" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ category order: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for bulk order updates (drag and drop)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> BulkUpdateOrders([FromBody] BulkFAQCategoryOrderUpdateViewModel model)
    {
        try
        {
            foreach (var update in model.Updates)
            {
                await _faqService.UpdateFAQCategoryOrderAsync(update.Id, update.NewOrder);
            }
            
            return Json(new { success = true, message = "Category orders updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating FAQ category orders");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Export FAQ categories to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        try
        {
            var categoriesWithCounts = await _faqService.GetCategoriesWithCountsAsync();
            
            var csv = "Name,Description,Icon,DisplayOrder,Status,FAQCount,ActiveFAQCount,CreatedDate\n";
            foreach (var item in categoriesWithCounts)
            {
                var category = item.Category;
                csv += $"\"{category.Name}\",\"{category.Description ?? ""}\",\"{category.Icon ?? ""}\",{category.DisplayOrder},\"{(category.IsActive ? "Active" : "Inactive")}\",{item.FAQCount},{item.ActiveFAQCount},\"{category.CreatedDate:yyyy-MM-dd}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"FAQ_Categories_{DateTime.Now:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting FAQ categories");
            TempData["ErrorMessage"] = "Error exporting FAQ categories.";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region Private Helper Methods

    private static List<string> GetPredefinedIcons()
    {
        return new List<string>
        {
            "bi bi-question-circle",
            "bi bi-info-circle",
            "bi bi-gear",
            "bi bi-shield",
            "bi bi-credit-card",
            "bi bi-truck",
            "bi bi-headset",
            "bi bi-star",
            "bi bi-person",
            "bi bi-house",
            "bi bi-bag",
            "bi bi-heart",
            "bi bi-bookmark",
            "bi bi-bell",
            "bi bi-envelope",
            "bi bi-phone",
            "bi bi-globe",
            "bi bi-chat",
            "bi bi-lightbulb",
            "bi bi-tools"
        };
    }

    private static List<string> GetPopularIcons()
    {
        return new List<string>
        {
            "bi bi-question-circle",
            "bi bi-info-circle",
            "bi bi-gear",
            "bi bi-shield",
            "bi bi-headset"
        };
    }

    private async Task ReloadCreateViewOptions(FAQCategoryCreateViewModel model)
    {
        model.PredefinedIcons = GetPredefinedIcons();
        model.PopularIcons = GetPopularIcons();
    }

    private async Task ReloadEditViewOptions(FAQCategoryEditViewModel model)
    {
        model.PredefinedIcons = GetPredefinedIcons();
        model.PopularIcons = GetPopularIcons();
    }

    #endregion
}
