using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for Category CRUD operations in Admin area
/// </summary>
[Area("Admin")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Display list of categories with search and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for filtering</param>
    /// <param name="page">Current page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Category list view</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int page = 1, int pageSize = 10)
    {
        ViewData["Title"] = "Categories";

        try
        {
            var (categories, totalCount) = await _categoryService.GetCategoriesAsync(searchTerm, page, pageSize);

            var viewModel = new CategoryListViewModel
            {
                Categories = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    ProductCount = c.Products?.Count ?? 0
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading categories: {ex.Message}";
            return View(new CategoryListViewModel());
        }
    }

    /// <summary>
    /// Display create category form
    /// </summary>
    /// <returns>Create view</returns>
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Category";
        
        var viewModel = new CategoryViewModel
        {
            IsActive = true,
            DisplayOrder = 0
        };

        return View(viewModel);
    }

    /// <summary>
    /// Process create category form submission
    /// </summary>
    /// <param name="model">Category view model</param>
    /// <returns>Redirect to index or return view with errors</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        ViewData["Title"] = "Create Category";

        if (ModelState.IsValid)
        {
            try
            {
                // Check if category name already exists
                if (await _categoryService.CategoryNameExistsAsync(model.Name))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(model);
                }

                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive
                };

                await _categoryService.CreateCategoryAsync(category);
                
                TempData["SuccessMessage"] = $"Category '{model.Name}' has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating category: {ex.Message}";
            }
        }

        return View(model);
    }

    /// <summary>
    /// Display edit category form
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Edit view or not found</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Category";

        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                ProductCount = category.Products?.Count ?? 0
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading category: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process edit category form submission
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="model">Category view model</param>
    /// <returns>Redirect to index or return view with errors</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        ViewData["Title"] = "Edit Category";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid category ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Check if category name already exists (excluding current category)
                if (await _categoryService.CategoryNameExistsAsync(model.Name, model.Id))
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    return View(model);
                }

                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update category properties
                category.Name = model.Name;
                category.Description = model.Description;
                category.ImageUrl = model.ImageUrl;
                category.DisplayOrder = model.DisplayOrder;
                category.IsActive = model.IsActive;

                await _categoryService.UpdateCategoryAsync(category);
                
                TempData["SuccessMessage"] = $"Category '{model.Name}' has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating category: {ex.Message}";
            }
        }

        return View(model);
    }

    /// <summary>
    /// Display delete category confirmation
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Delete confirmation view or not found</returns>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Category";

        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CategoryDeleteViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products?.Count ?? 0,
                CreatedDate = category.CreatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading category: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process category deletion
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Redirect to index with success or error message</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var categoryName = category.Name;
            var hasProducts = category.Products?.Any() ?? false;

            if (await _categoryService.DeleteCategoryAsync(id))
            {
                if (hasProducts)
                {
                    TempData["SuccessMessage"] = $"Category '{categoryName}' has been deactivated successfully (contains products).";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Category '{categoryName}' has been deleted successfully.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete category.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// AJAX endpoint to check if category name exists
    /// </summary>
    /// <param name="name">Category name to check</param>
    /// <param name="id">Current category ID (for edit scenarios)</param>
    /// <returns>JSON result indicating availability</returns>
    [HttpGet]
    public async Task<IActionResult> CheckCategoryName(string name, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Json(new { available = false });
        }

        var exists = await _categoryService.CategoryNameExistsAsync(name, id);
        return Json(new { available = !exists });
    }
}
