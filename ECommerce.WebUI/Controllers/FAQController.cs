using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Public FAQ controller for customer-facing pages
/// </summary>
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
    /// Display public FAQ page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? categoryId, string? search)
    {
        ViewData["Title"] = "Frequently Asked Questions";

        try
        {
            var categories = await _faqService.GetFAQCategoriesAsync();
            var faqs = await _faqService.GetActiveFAQsAsync(categoryId);

            if (!string.IsNullOrEmpty(search))
            {
                faqs = await _faqService.SearchFAQsAsync(search, categoryId, 50);
            }

            var model = new FaqPageViewModel
            {
                Faqs = faqs.ToList(),
                Categories = categories.ToList(),
                SelectedCategoryId = categoryId,
                Search = search
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading public FAQ page");
            return View(new FaqPageViewModel());
        }
    }

    /// <summary>
    /// View individual FAQ and increment view count
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var faq = await _faqService.GetFAQByIdAsync(id);
            
            if (faq == null || !faq.IsActive)
            {
                return NotFound();
            }

            // Increment view count
            await _faqService.IncrementViewCountAsync(id);

            ViewData["Title"] = faq.Question;
            return View(faq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading FAQ details for ID: {Id}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Mark FAQ as helpful or not helpful
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkHelpful(int id, bool isHelpful)
    {
        try
        {
            var success = await _faqService.MarkFAQHelpfulnessAsync(id, isHelpful);
            
            if (success)
            {
                return Json(new { success = true, message = "Thank you for your feedback!" });
            }
            
            return Json(new { success = false, message = "Unable to record feedback" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking FAQ helpfulness for ID: {Id}", id);
            return Json(new { success = false, message = "Error recording feedback" });
        }
    }

    /// <summary>
    /// Search FAQs (AJAX)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string query, int? categoryId)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>());
            }

            var results = await _faqService.SearchFAQsAsync(query, categoryId, 10);
            
            var searchResults = results.Select(f => new
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer.Length > 100 ? f.Answer[..100] + "..." : f.Answer,
                CategoryName = f.Category?.Name,
                ViewCount = f.ViewCount
            });

            return Json(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching FAQs with query: {Query}", query);
            return Json(new List<object>());
        }
    }
}

