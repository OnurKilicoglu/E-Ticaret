using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Controller for product catalog and category pages
/// Routes: /category/{slug?}, /catalog, /search
/// </summary>
public class CatalogController : Controller
{
    private readonly ILogger<CatalogController> _logger;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public CatalogController(
        ILogger<CatalogController> logger,
        IProductService productService,
        ICategoryService categoryService)
    {
        _logger = logger;
        _productService = productService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Main catalog page with all products
    /// GET: /catalog
    /// </summary>
    [HttpGet]
    [Route("catalog")]
    public async Task<IActionResult> Index(CatalogFiltersViewModel filters)
    {
        try
        {
            var viewModel = new CatalogViewModel
            {
                Filters = filters
            };

            // Load all categories for sidebar
            var categories = await _categoryService.GetActiveCategoriesAsync();
            viewModel.Categories = categories.Select(c => new PublicCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = GenerateSlug(c.Name)
            }).ToList();

            // Load products with filters
            var (products, totalCount) = await _productService.GetProductsAsync(
                searchTerm: filters.SearchTerm,
                categoryId: filters.CategoryId,
                sortBy: filters.SortBy,
                sortOrder: filters.SortOrder,
                page: filters.Page,
                pageSize: filters.PageSize
            );

            viewModel.Products = new PagedProductListViewModel
            {
                Products = products.Select(MapToProductCard).ToList(),
                CurrentPage = filters.Page,
                TotalItems = totalCount,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };

            // SEO
            ViewData["Title"] = "All Products";
            ViewData["MetaDescription"] = "Browse our complete catalog of products with advanced filtering options.";
            ViewData["CanonicalUrl"] = Url.Action("Index", "Catalog", null, Request.Scheme);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading catalog");
            return View(new CatalogViewModel());
        }
    }

    /// <summary>
    /// Category page with filtered products
    /// GET: /category/{slug}
    /// </summary>
    [HttpGet]
    [Route("category/{slug}")]
    public async Task<IActionResult> Category(string slug, CatalogFiltersViewModel filters)
    {
        try
        {
            // Find category by slug (generated from name)
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var category = categories.FirstOrDefault(c => GenerateSlug(c.Name).Equals(slug, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CatalogViewModel
            {
                CurrentCategory = new PublicCategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = GenerateSlug(category.Name),
                    Description = category.Description
                },
                Filters = filters
            };

            // Set category filter
            viewModel.Filters.CategoryId = category.Id;

            // Load all categories for sidebar
            viewModel.Categories = categories.Select(c => new PublicCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = GenerateSlug(c.Name)
            }).ToList();

            // Load products in this category
            var (products, totalCount) = await _productService.GetProductsAsync(
                searchTerm: filters.SearchTerm,
                categoryId: category.Id,
                sortBy: filters.SortBy,
                sortOrder: filters.SortOrder,
                page: filters.Page,
                pageSize: filters.PageSize
            );

            viewModel.Products = new PagedProductListViewModel
            {
                Products = products.Select(MapToProductCard).ToList(),
                CurrentPage = filters.Page,
                TotalItems = totalCount,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };

            // SEO
            ViewData["Title"] = $"{category.Name} - Products";
            ViewData["MetaDescription"] = $"Browse {category.Name} products. {category.Description}";
            ViewData["CanonicalUrl"] = Url.Action("Category", "Catalog", new { slug }, Request.Scheme);

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category: {Slug}", slug);
            return NotFound();
        }
    }

    /// <summary>
    /// Search products
    /// GET: /search
    /// </summary>
    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search(string? q, CatalogFiltersViewModel filters)
    {
        try
        {
            if (string.IsNullOrEmpty(q))
            {
                return RedirectToAction("Index");
            }

            var viewModel = new CatalogViewModel
            {
                Filters = filters
            };

            viewModel.Filters.SearchTerm = q;

            // Load all categories for sidebar
            var categories = await _categoryService.GetActiveCategoriesAsync();
            viewModel.Categories = categories.Select(c => new PublicCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = GenerateSlug(c.Name)
            }).ToList();

            // Search products
            var (products, totalCount) = await _productService.GetProductsAsync(
                searchTerm: q,
                categoryId: filters.CategoryId,
                sortBy: filters.SortBy,
                sortOrder: filters.SortOrder,
                page: filters.Page,
                pageSize: filters.PageSize
            );

            viewModel.Products = new PagedProductListViewModel
            {
                Products = products.Select(MapToProductCard).ToList(),
                CurrentPage = filters.Page,
                TotalItems = totalCount,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };

            // SEO
            ViewData["Title"] = $"Search Results for \"{q}\"";
            ViewData["MetaDescription"] = $"Search results for {q}. Found {totalCount} products.";

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products: {Query}", q);
            return View("Index", new CatalogViewModel());
        }
    }

    /// <summary>
    /// AJAX endpoint for quick search suggestions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> QuickSearch(string? q)
    {
        try
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
            {
                return Json(new { suggestions = new List<object>() });
            }

            var (products, _) = await _productService.GetProductsAsync(
                searchTerm: q,
                page: 1,
                pageSize: 5
            );

            var suggestions = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price.ToString("C"),
                image = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/product-placeholder.jpg",
                url = Url.Action("DetailsBySlug", "Product", new { slug = GenerateSlug(p.Name) })
            }).ToList();

            return Json(new { suggestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quick search: {Query}", q);
            return Json(new { suggestions = new List<object>() });
        }
    }

    /// <summary>
    /// Helper method to map Product entity to ProductCardViewModel
    /// </summary>
    private ProductCardViewModel MapToProductCard(ECommerce.Core.Entities.Product product)
    {
        return new ProductCardViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = GenerateSlug(product.Name), // Generate slug from name since Product doesn't have Slug
            Description = product.Description ?? "",
            ShortDescription = (product.Description?.Length > 100
                ? product.Description.Substring(0, 97) + "..."
                : product.Description) ?? "",
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            ImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/product-placeholder.jpg",
            StockQuantity = product.StockQuantity,
            CategoryName = product.Category?.Name ?? "Uncategorized",
            IsFeatured = product.IsFeatured,
            CreatedDate = product.CreatedDate
        };
    }

    /// <summary>
    /// Generate URL-friendly slug from text
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
