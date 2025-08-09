using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for FAQ management in Admin area
/// </summary>
[Area("Admin")]
public class FAQController : Controller
{
    private readonly IFAQService _faqService;
    private readonly ILogger<FAQController> _logger;

    public FAQController(IFAQService faqService, ILogger<FAQController> logger)
    {
        _faqService = faqService;
        _logger = logger;
    }

    /// <summary>
    /// Display list of FAQs with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, bool? isActive,
        string? tags, string? author, string sortBy = "displayOrder", string sortOrder = "asc", 
        int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "FAQ Management";

        try
        {
            var (faqs, totalCount) = await _faqService.GetFAQsAsync(
                searchTerm, categoryId, isActive, tags, author, sortBy, sortOrder, page, pageSize);

            var statistics = await _faqService.GetFAQStatisticsAsync();
            var categories = await _faqService.GetFAQCategoriesAsync(includeInactive: true);
            var categoriesWithCounts = await _faqService.GetCategoriesWithCountsAsync();

            // Get unique authors and tags
            var allFAQs = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            var authors = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Author))
                                     .Select(f => f.Author!)
                                     .Distinct()
                                     .OrderBy(a => a);

            var tagOptions = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Tags))
                                         .SelectMany(f => f.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                         .Select(t => t.Trim())
                                         .Distinct()
                                         .OrderBy(t => t);

            var viewModel = new FAQListViewModel
            {
                FAQs = faqs.Select(f => new FAQItemViewModel
                {
                    Id = f.Id,
                    Question = f.Question,
                    Answer = f.Answer,
                    CategoryId = f.CategoryId,
                    CategoryName = f.Category?.Name,
                    DisplayOrder = f.DisplayOrder,
                    IsActive = f.IsActive,
                    ViewCount = f.ViewCount,
                    HelpfulCount = f.HelpfulCount,
                    NotHelpfulCount = f.NotHelpfulCount,
                    Tags = f.Tags,
                    Author = f.Author,
                    CreatedDate = f.CreatedDate,
                    UpdatedDate = f.UpdatedDate
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                IsActive = isActive,
                Tags = tags,
                Author = author,
                SortBy = sortBy,
                SortOrder = sortOrder,
                CategoryOptions = categories.Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(), 
                    Text = c.Name 
                }).ToList(),
                AuthorOptions = authors.Select(a => new SelectListItem 
                { 
                    Value = a, 
                    Text = a 
                }).ToList(),
                TagOptions = tagOptions.ToList(),
                Statistics = new FAQStatisticsViewModel
                {
                    TotalFAQs = statistics.TotalFAQs,
                    ActiveFAQs = statistics.ActiveFAQs,
                    InactiveFAQs = statistics.InactiveFAQs,
                    TotalCategories = statistics.TotalCategories,
                    ActiveCategories = statistics.ActiveCategories,
                    TotalViews = statistics.TotalViews,
                    TotalHelpfulVotes = statistics.TotalHelpfulVotes,
                    TotalNotHelpfulVotes = statistics.TotalNotHelpfulVotes,
                    MostViewedFAQQuestion = statistics.MostViewedFAQ?.Question,
                    MostViewedFAQViews = statistics.MostViewedFAQ?.ViewCount ?? 0,
                    MostHelpfulFAQQuestion = statistics.MostHelpfulFAQ?.Question,
                    MostHelpfulFAQScore = (statistics.MostHelpfulFAQ?.HelpfulCount ?? 0) - (statistics.MostHelpfulFAQ?.NotHelpfulCount ?? 0),
                    MostPopularCategoryName = statistics.MostPopularCategory?.Name,
                    MostPopularCategoryCount = categoriesWithCounts.FirstOrDefault(c => c.Category.Id == statistics.MostPopularCategory?.Id)?.FAQCount ?? 0,
                    LastUpdated = statistics.LastUpdated
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ index");
            TempData["ErrorMessage"] = $"Error loading FAQs: {ex.Message}";
            return View(new FAQListViewModel());
        }
    }

    /// <summary>
    /// Display detailed FAQ information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "FAQ Details";

        try
        {
            var faq = await _faqService.GetFAQByIdAsync(id);
            if (faq == null)
            {
                TempData["ErrorMessage"] = "FAQ not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FAQDetailViewModel
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                CategoryId = faq.CategoryId,
                CategoryName = faq.Category?.Name,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive,
                ViewCount = faq.ViewCount,
                HelpfulCount = faq.HelpfulCount,
                NotHelpfulCount = faq.NotHelpfulCount,
                Tags = faq.Tags,
                Author = faq.Author,
                CreatedDate = faq.CreatedDate,
                UpdatedDate = faq.UpdatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ details for ID: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading FAQ details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display FAQ creation form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create New FAQ";

        try
        {
            var categories = await _faqService.GetFAQCategoriesAsync();
            var allFAQs = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            
            var authors = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Author))
                                     .Select(f => f.Author!)
                                     .Distinct()
                                     .OrderBy(a => a);

            var tagOptions = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Tags))
                                         .SelectMany(f => f.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                         .Select(t => t.Trim())
                                         .Distinct()
                                         .OrderBy(t => t);

            var viewModel = new FAQCreateViewModel
            {
                Author = User.Identity?.Name ?? "Admin",
                DisplayOrder = await _faqService.GetNextFAQDisplayOrderAsync(),
                CategoryOptions = categories.Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(), 
                    Text = c.Name 
                }).ToList(),
                AuthorOptions = authors.Select(a => new SelectListItem 
                { 
                    Value = a, 
                    Text = a 
                }).ToList(),
                TagOptions = tagOptions.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ create form");
            TempData["ErrorMessage"] = $"Error loading create form: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process FAQ creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FAQCreateViewModel model)
    {
        ViewData["Title"] = "Create New FAQ";

        if (ModelState.IsValid)
        {
            try
            {
                var faq = new FAQ
                {
                    Question = model.Question,
                    Answer = model.Answer,
                    CategoryId = model.CategoryId,
                    DisplayOrder = model.DisplayOrder > 0 ? model.DisplayOrder : await _faqService.GetNextFAQDisplayOrderAsync(model.CategoryId),
                    IsActive = model.IsActive,
                    Tags = model.Tags,
                    Author = model.Author
                };

                var createdFAQ = await _faqService.CreateFAQAsync(faq);

                if (createdFAQ != null)
                {
                    TempData["SuccessMessage"] = $"FAQ '{model.Question}' has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = createdFAQ.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create FAQ.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FAQ");
                TempData["ErrorMessage"] = $"Error creating FAQ: {ex.Message}";
            }
        }

        // Reload dropdown options if model is invalid
        await ReloadCreateEditOptions(model);
        return View(model);
    }

    /// <summary>
    /// Display FAQ edit form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit FAQ";

        try
        {
            var faq = await _faqService.GetFAQByIdAsync(id);
            if (faq == null)
            {
                TempData["ErrorMessage"] = "FAQ not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _faqService.GetFAQCategoriesAsync();
            var allFAQs = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            
            var authors = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Author))
                                     .Select(f => f.Author!)
                                     .Distinct()
                                     .OrderBy(a => a);

            var tagOptions = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Tags))
                                         .SelectMany(f => f.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                         .Select(t => t.Trim())
                                         .Distinct()
                                         .OrderBy(t => t);

            var viewModel = new FAQEditViewModel
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                CategoryId = faq.CategoryId,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive,
                Tags = faq.Tags,
                Author = faq.Author,
                CreatedDate = faq.CreatedDate,
                ViewCount = faq.ViewCount,
                HelpfulCount = faq.HelpfulCount,
                NotHelpfulCount = faq.NotHelpfulCount,
                CategoryOptions = categories.Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(), 
                    Text = c.Name,
                    Selected = c.Id == faq.CategoryId
                }).ToList(),
                AuthorOptions = authors.Select(a => new SelectListItem 
                { 
                    Value = a, 
                    Text = a,
                    Selected = a == faq.Author
                }).ToList(),
                TagOptions = tagOptions.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ for edit: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading FAQ: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process FAQ update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FAQEditViewModel model)
    {
        ViewData["Title"] = "Edit FAQ";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid FAQ ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                var faq = new FAQ
                {
                    Id = model.Id,
                    Question = model.Question,
                    Answer = model.Answer,
                    CategoryId = model.CategoryId,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    Tags = model.Tags,
                    Author = model.Author
                };

                var success = await _faqService.UpdateFAQAsync(faq);

                if (success)
                {
                    TempData["SuccessMessage"] = $"FAQ '{model.Question}' has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update FAQ.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ: {Id}", id);
                TempData["ErrorMessage"] = $"Error updating FAQ: {ex.Message}";
            }
        }

        // Reload dropdown options if model is invalid
        await ReloadEditOptions(model);
        return View(model);
    }

    /// <summary>
    /// Display FAQ deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete FAQ";

        try
        {
            var faq = await _faqService.GetFAQByIdAsync(id);
            if (faq == null)
            {
                TempData["ErrorMessage"] = "FAQ not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FAQDeleteViewModel
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                CategoryName = faq.Category?.Name,
                IsActive = faq.IsActive,
                ViewCount = faq.ViewCount,
                HelpfulCount = faq.HelpfulCount,
                NotHelpfulCount = faq.NotHelpfulCount,
                Tags = faq.Tags,
                CreatedDate = faq.CreatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ for delete: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading FAQ: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process FAQ deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, FAQDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                bool success;
                
                if (model.HardDelete)
                {
                    success = await _faqService.HardDeleteFAQAsync(id);
                    TempData["SuccessMessage"] = $"FAQ '{model.Question}' has been permanently deleted.";
                }
                else
                {
                    success = await _faqService.DeleteFAQAsync(id);
                    TempData["SuccessMessage"] = $"FAQ '{model.Question}' has been deactivated.";
                }

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete FAQ.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FAQ: {Id}", id);
            TempData["ErrorMessage"] = $"Error deleting FAQ: {ex.Message}";
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
            var success = await _faqService.ToggleFAQStatusAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "FAQ status updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update FAQ status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling FAQ status: {Id}", id);
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
            var success = await _faqService.UpdateFAQOrderAsync(id, newOrder);
            
            if (success)
            {
                return Json(new { success = true, message = "FAQ order updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update FAQ order" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ order: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for bulk order updates (drag and drop)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> BulkUpdateOrders([FromBody] BulkFAQOrderUpdateViewModel model)
    {
        try
        {
            var orderUpdates = model.Updates.ToDictionary(u => u.Id, u => u.NewOrder);
            var success = await _faqService.BulkUpdateFAQOrdersAsync(orderUpdates);
            
            if (success)
            {
                return Json(new { success = true, message = "FAQ orders updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update FAQ orders" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating FAQ orders");
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get tag suggestions for autocomplete
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTagSuggestions()
    {
        try
        {
            var allFAQs = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            var tags = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Tags))
                                   .SelectMany(f => f.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                   .Select(t => t.Trim())
                                   .Distinct()
                                   .OrderBy(t => t)
                                   .Take(20);
            return Json(tags);
        }
        catch
        {
            return Json(new string[0]);
        }
    }

    /// <summary>
    /// Export FAQs to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        try
        {
            var (faqs, _) = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            
            var csv = "Question,Answer,Category,Tags,Author,Status,Views,Helpful,NotHelpful,Created\n";
            foreach (var faq in faqs)
            {
                csv += $"\"{faq.Question}\",\"{faq.Answer}\",\"{faq.Category?.Name ?? ""}\",\"{faq.Tags ?? ""}\",\"{faq.Author ?? ""}\",\"{(faq.IsActive ? "Active" : "Inactive")}\",{faq.ViewCount},{faq.HelpfulCount},{faq.NotHelpfulCount},\"{faq.CreatedDate:yyyy-MM-dd}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"FAQs_{DateTime.Now:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting FAQs");
            TempData["ErrorMessage"] = "Error exporting FAQs.";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task ReloadCreateEditOptions(FAQCreateViewModel model)
    {
        try
        {
            var categories = await _faqService.GetFAQCategoriesAsync();
            var allFAQs = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            
            var authors = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Author))
                                     .Select(f => f.Author!)
                                     .Distinct()
                                     .OrderBy(a => a);

            var tagOptions = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Tags))
                                         .SelectMany(f => f.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                         .Select(t => t.Trim())
                                         .Distinct()
                                         .OrderBy(t => t);

            model.CategoryOptions = categories.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();
            model.AuthorOptions = authors.Select(a => new SelectListItem 
            { 
                Value = a, 
                Text = a 
            }).ToList();
            model.TagOptions = tagOptions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading create options");
        }
    }

    private async Task ReloadEditOptions(FAQEditViewModel model)
    {
        try
        {
            var categories = await _faqService.GetFAQCategoriesAsync();
            var allFAQs = await _faqService.GetFAQsAsync(pageSize: int.MaxValue);
            
            var authors = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Author))
                                     .Select(f => f.Author!)
                                     .Distinct()
                                     .OrderBy(a => a);

            var tagOptions = allFAQs.FAQs.Where(f => !string.IsNullOrEmpty(f.Tags))
                                         .SelectMany(f => f.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                         .Select(t => t.Trim())
                                         .Distinct()
                                         .OrderBy(t => t);

            model.CategoryOptions = categories.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            }).ToList();
            model.AuthorOptions = authors.Select(a => new SelectListItem 
            { 
                Value = a, 
                Text = a,
                Selected = a == model.Author
            }).ToList();
            model.TagOptions = tagOptions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading edit options");
        }
    }

    #endregion
}
