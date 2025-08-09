using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Controller for individual product details
/// Routes: /product/{slug}
/// </summary>
public class ProductController : Controller
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductController(
        ILogger<ProductController> logger,
        IProductService productService,
        ICategoryService categoryService)
    {
        _logger = logger;
        _productService = productService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Product details page by ID
    /// GET: /product/{id:int}
    /// </summary>
    [HttpGet]
    [Route("product/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Redirect to slug-based URL for SEO
            var slug = GenerateSlug(product.Name);
            return RedirectToAction("DetailsBySlug", new { slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product with ID {ProductId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Product details page by slug
    /// GET: /product/{slug}
    /// </summary>
    [HttpGet]
    [Route("product/{slug}")]
    public async Task<IActionResult> DetailsBySlug(string slug)
    {
        try
        {
            // Find product by slug (generated from name)
            var products = await _productService.GetActiveProductsAsync();
            var product = products.FirstOrDefault(p => GenerateSlug(p.Name).Equals(slug, StringComparison.OrdinalIgnoreCase));

            if (product == null)
            {
                return NotFound();
            }

            // Get full product details
            var fullProduct = await _productService.GetProductByIdAsync(product.Id);
            if (fullProduct == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailViewModel
            {
                Id = fullProduct.Id,
                Name = fullProduct.Name,
                Slug = GenerateSlug(fullProduct.Name),
                Description = fullProduct.Description ?? "",
                LongDescription = fullProduct.Description, // Use Description as LongDescription since no separate field exists
                Price = fullProduct.Price,
                DiscountPrice = fullProduct.DiscountPrice,
                SKU = fullProduct.SKU ?? "",
                StockQuantity = fullProduct.StockQuantity,
                CategoryName = fullProduct.Category?.Name ?? "Uncategorized",
                CategorySlug = GenerateSlug(fullProduct.Category?.Name ?? ""),
                ImageUrls = fullProduct.ProductImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(),
                CreatedDate = fullProduct.CreatedDate,
                MetaTitle = $"{fullProduct.Name} - Buy Online",
                MetaDescription = (fullProduct.Description?.Length > 160 ? fullProduct.Description.Substring(0, 157) + "..." : fullProduct.Description) ?? "",
                CanonicalUrl = Url.Action("Details", "Product", new { slug }, Request.Scheme) ?? ""
            };

            // If no images, add placeholder
            if (!viewModel.ImageUrls.Any())
            {
                viewModel.ImageUrls.Add("/images/product-placeholder.jpg");
            }

            // Load related products from same category
            if (fullProduct.CategoryId > 0)
            {
                var (relatedProducts, _) = await _productService.GetProductsAsync(
                    categoryId: fullProduct.CategoryId,
                    page: 1,
                    pageSize: 8
                );

                viewModel.RelatedProducts = relatedProducts
                    .Where(p => p.Id != fullProduct.Id) // Exclude current product
                    .Select(p => new ProductCardViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Slug = GenerateSlug(p.Name),
                        Description = p.Description ?? "",
                        ShortDescription = (p.Description?.Length > 100
                            ? p.Description.Substring(0, 97) + "..."
                            : p.Description) ?? "",
                        Price = p.Price,
                        DiscountPrice = p.DiscountPrice,
                        ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/product-placeholder.jpg",
                        StockQuantity = p.StockQuantity,
                        CategoryName = p.Category?.Name ?? "Uncategorized",
                        IsFeatured = p.IsFeatured,
                        CreatedDate = p.CreatedDate
                    })
                    .Take(4)
                    .ToList();
            }

            // Increment view count (if this functionality exists in the service)
            try
            {
                // Note: This would require adding a method to IProductService
                // await _productService.IncrementViewCountAsync(fullProduct.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not increment view count for product {ProductId}", fullProduct.Id);
            }

            // SEO
            ViewData["Title"] = viewModel.MetaTitle;
            ViewData["MetaDescription"] = viewModel.MetaDescription;
            ViewData["CanonicalUrl"] = viewModel.CanonicalUrl;
            
            // Structured data for rich snippets
            ViewData["StructuredData"] = CreateProductStructuredData(viewModel);

            return View("Details", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product details: {Slug}", slug);
            return NotFound();
        }
    }

    /// <summary>
    /// AJAX endpoint to check stock availability
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckStock(int productId, int quantity = 1)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new { available = false, message = "Product not found" });
            }

            var available = product.StockQuantity >= quantity;
            var message = available 
                ? $"{product.StockQuantity} items available"
                : product.StockQuantity == 0 
                    ? "Out of stock" 
                    : $"Only {product.StockQuantity} items available";

            return Json(new { 
                available, 
                message, 
                stockQuantity = product.StockQuantity,
                requestedQuantity = quantity 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock for product {ProductId}", productId);
            return Json(new { available = false, message = "Error checking stock" });
        }
    }

    /// <summary>
    /// Create structured data (JSON-LD) for product
    /// </summary>
    private string CreateProductStructuredData(ProductDetailViewModel product)
    {
        var structuredData = new
        {
            context = "https://schema.org/",
            type = "Product",
            name = product.Name,
            description = product.Description,
            sku = product.SKU,
            image = product.ImageUrls.Select(url => $"{Request.Scheme}://{Request.Host}{url}").ToArray(),
            offers = new
            {
                type = "Offer",
                url = product.CanonicalUrl,
                priceCurrency = "USD",
                price = product.HasDiscount ? product.DiscountPrice : product.Price,
                availability = product.InStock ? "https://schema.org/InStock" : "https://schema.org/OutOfStock",
                seller = new
                {
                    type = "Organization",
                    name = "E-Commerce Store"
                }
            },
            brand = new
            {
                type = "Brand",
                name = "E-Commerce Store"
            },
            category = product.CategoryName
        };

        return System.Text.Json.JsonSerializer.Serialize(structuredData, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
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
