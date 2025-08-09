using Microsoft.AspNetCore.Mvc;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Models;

namespace ECommerce.WebUI.Components;

/// <summary>
/// View component for rendering category navigation in the header
/// </summary>
public class CategoryNavigationViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;

    public CategoryNavigationViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            
            var viewModel = categories.Select(c => new CategoryNavigationViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = GenerateSlug(c.Name)
            }).ToList();

            return View(viewModel);
        }
        catch (Exception)
        {
            // Return empty list on error to prevent layout breaking
            return View(new List<CategoryNavigationViewModel>());
        }
    }

    /// <summary>
    /// Generate URL-friendly slug from category name
    /// </summary>
    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("&", "and")
                   .Replace("'", "")
                   .Replace("\"", "")
                   .Replace("/", "-")
                   .Replace("\\", "-")
                   .Replace(".", "")
                   .Replace(",", "")
                   .Replace("(", "")
                   .Replace(")", "")
                   .Replace("[", "")
                   .Replace("]", "")
                   .Replace("{", "")
                   .Replace("}", "")
                   .Replace(":", "")
                   .Replace(";", "")
                   .Replace("!", "")
                   .Replace("?", "")
                   .Replace("#", "")
                   .Replace("@", "")
                   .Replace("%", "")
                   .Replace("^", "")
                   .Replace("*", "")
                   .Replace("+", "")
                   .Replace("=", "")
                   .Replace("|", "")
                   .Replace("<", "")
                   .Replace(">", "")
                   .Replace("~", "")
                   .Replace("`", "");
    }
}


