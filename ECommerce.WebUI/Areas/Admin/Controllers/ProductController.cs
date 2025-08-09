using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using ECommerce.WebUI.Services;
using ECommerce.WebUI.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for Product CRUD operations in Admin area
/// </summary>
[Area("Admin")]
[AdminAuthorize]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IFileUploadService _fileUploadService;

    public ProductController(IProductService productService, ICategoryService categoryService, IFileUploadService fileUploadService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// Display list of products with search, filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string sortBy = "name", 
        string sortOrder = "asc", int page = 1, int pageSize = 10)
    {
        ViewData["Title"] = "Products";

        try
        {
            var (products, totalCount) = await _productService.GetProductsAsync(
                searchTerm, categoryId, sortBy, sortOrder, page, pageSize);

            var categories = await _categoryService.GetActiveCategoriesAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.ImageUrl,
                    SKU = p.SKU,
                    Brand = p.Brand,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    ViewCount = p.ViewCount,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading products: {ex.Message}";
            return View(new ProductListViewModel());
        }
    }

    /// <summary>
    /// Display product details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Product Details";

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                CategoryName = product.Category.Name,
                ImageUrl = product.ImageUrl,
                SKU = product.SKU,
                Brand = product.Brand,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                ViewCount = product.ViewCount,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate,
                ProductImages = product.ProductImages.Select(pi => new ProductImageViewModel
                {
                    Id = pi.Id,
                    ProductId = pi.ProductId,
                    ImageUrl = pi.ImageUrl,
                    AltText = pi.AltText,
                    IsMain = pi.IsMain,
                    DisplayOrder = pi.DisplayOrder
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display create product form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create Product";

        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            
            var viewModel = new ProductViewModel
            {
                IsActive = true,
                IsFeatured = false,
                StockQuantity = 0,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading create form: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process create product form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        ViewData["Title"] = "Create Product";

        try
        {
            // Reload categories for dropdown in case of validation errors
            var categories = await _categoryService.GetActiveCategoriesAsync();
            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            if (ModelState.IsValid)
            {
                // Validate discount price
                if (model.DiscountPrice.HasValue && model.DiscountPrice >= model.Price)
                {
                    ModelState.AddModelError("DiscountPrice", "Discount price must be less than regular price.");
                    return View(model);
                }

                // Check if product name already exists
                if (await _productService.ProductNameExistsAsync(model.Name))
                {
                    ModelState.AddModelError("Name", "A product with this name already exists.");
                    return View(model);
                }

                // Check if SKU already exists (if provided)
                if (!string.IsNullOrWhiteSpace(model.SKU) && await _productService.SkuExistsAsync(model.SKU))
                {
                    ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                    return View(model);
                }

                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    DiscountPrice = model.DiscountPrice,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    ImageUrl = model.ImageUrl,
                    SKU = model.SKU,
                    Brand = model.Brand,
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured
                };

                var createdProduct = await _productService.CreateProductAsync(product);

                // Handle file uploads
                if (model.ImageFiles != null && model.ImageFiles.Any())
                {
                    await ProcessImageUploadsAsync(createdProduct.Id, model.ImageFiles, model.MainImageId);
                }
                
                TempData["SuccessMessage"] = $"Product '{model.Name}' has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating product: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Display edit product form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Product";

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryService.GetActiveCategoriesAsync();

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ImageUrl = product.ImageUrl,
                SKU = product.SKU,
                Brand = product.Brand,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                ViewCount = product.ViewCount,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == product.CategoryId
                }).ToList(),
                ProductImages = product.ProductImages.Select(pi => new ProductImageViewModel
                {
                    Id = pi.Id,
                    ProductId = pi.ProductId,
                    ImageUrl = pi.ImageUrl,
                    AltText = pi.AltText,
                    IsMain = pi.IsMain,
                    DisplayOrder = pi.DisplayOrder
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process edit product form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        ViewData["Title"] = "Edit Product";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid product ID.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Reload categories for dropdown in case of validation errors
            var categories = await _categoryService.GetActiveCategoriesAsync();
            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            }).ToList();

            if (ModelState.IsValid)
            {
                // Validate discount price
                if (model.DiscountPrice.HasValue && model.DiscountPrice >= model.Price)
                {
                    ModelState.AddModelError("DiscountPrice", "Discount price must be less than regular price.");
                    return View(model);
                }

                // Check if product name already exists (excluding current product)
                if (await _productService.ProductNameExistsAsync(model.Name, model.Id))
                {
                    ModelState.AddModelError("Name", "A product with this name already exists.");
                    return View(model);
                }

                // Check if SKU already exists (excluding current product)
                if (!string.IsNullOrWhiteSpace(model.SKU) && await _productService.SkuExistsAsync(model.SKU, model.Id))
                {
                    ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                    return View(model);
                }

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update product properties
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.DiscountPrice = model.DiscountPrice;
                product.StockQuantity = model.StockQuantity;
                product.CategoryId = model.CategoryId;
                product.ImageUrl = model.ImageUrl;
                product.SKU = model.SKU;
                product.Brand = model.Brand;
                product.IsActive = model.IsActive;
                product.IsFeatured = model.IsFeatured;

                // Handle image deletions
                if (model.ImagesToDelete != null && model.ImagesToDelete.Any())
                {
                    await DeleteProductImagesAsync(product.Id, model.ImagesToDelete);
                }

                // Handle new file uploads
                if (model.ImageFiles != null && model.ImageFiles.Any())
                {
                    await ProcessImageUploadsAsync(product.Id, model.ImageFiles, model.MainImageId);
                }

                // Handle main image selection
                if (model.MainImageId.HasValue)
                {
                    await SetMainImageAsync(product.Id, model.MainImageId.Value);
                }

                await _productService.UpdateProductAsync(product);
                
                TempData["SuccessMessage"] = $"Product '{model.Name}' has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating product: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Display delete product confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Product";

        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check order count for smart delete logic
            var orderCount = product.OrderItems?.Count ?? 0;

            var viewModel = new ProductDeleteViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                CategoryName = product.Category.Name,
                ImageUrl = product.ImageUrl,
                SKU = product.SKU,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                OrderCount = orderCount
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process product deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, bool hardDelete = false)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            var productName = product.Name;
            var hasOrders = product.OrderItems?.Any() ?? false;

            bool success;
            if (hardDelete && !hasOrders)
            {
                success = await _productService.HardDeleteProductAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Product '{productName}' has been permanently deleted.";
                }
            }
            else
            {
                success = await _productService.DeleteProductAsync(id);
                if (success)
                {
                    if (hasOrders)
                    {
                        TempData["SuccessMessage"] = $"Product '{productName}' has been deactivated (has order history).";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Product '{productName}' has been deactivated successfully.";
                    }
                }
            }

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to delete product.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// AJAX endpoint to check if product name exists
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckProductName(string name, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Json(new { available = false });
        }

        var exists = await _productService.ProductNameExistsAsync(name, id);
        return Json(new { available = !exists });
    }

    /// <summary>
    /// AJAX endpoint to check if SKU exists
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckSku(string sku, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return Json(new { available = true });
        }

        var exists = await _productService.SkuExistsAsync(sku, id);
        return Json(new { available = !exists });
    }

    /// <summary>
    /// AJAX endpoint to update stock quantity
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateStock(int productId, int quantity)
    {
        try
        {
            var success = await _productService.UpdateStockAsync(productId, quantity);
            if (success)
            {
                return Json(new { success = true, message = "Stock updated successfully." });
            }
            
            return Json(new { success = false, message = "Product not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Process uploaded images for a product
    /// </summary>
    private async Task ProcessImageUploadsAsync(int productId, List<IFormFile> images, int? mainImageId)
    {
        if (images == null || !images.Any())
            return;

        try
        {
            var uploadedFiles = await _fileUploadService.UploadProductImagesAsync(images, productId);
            var product = await _productService.GetProductByIdAsync(productId);
            
            if (product != null)
            {
                var displayOrder = product.ProductImages.Count;
                var isFirstImage = !product.ProductImages.Any();

                foreach (var uploadedFile in uploadedFiles)
                {
                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = uploadedFile.WebPath,
                        AltText = $"{product.Name} image",
                        IsMain = isFirstImage, // First image is main by default
                        DisplayOrder = displayOrder++
                    };

                    product.ProductImages.Add(productImage);
                    isFirstImage = false;
                }

                // Set main image URL for quick access
                if (product.ProductImages.Any() && string.IsNullOrEmpty(product.ImageUrl))
                {
                    var mainImage = product.ProductImages.FirstOrDefault(pi => pi.IsMain) ?? product.ProductImages.First();
                    product.ImageUrl = mainImage.ImageUrl;
                }

                await _productService.UpdateProductAsync(product);
            }
        }
        catch (Exception ex)
        {
            // Log error
            TempData["ErrorMessage"] = $"Error uploading images: {ex.Message}";
        }
    }

    /// <summary>
    /// Delete product images
    /// </summary>
    private async Task DeleteProductImagesAsync(int productId, List<int> imageIds)
    {
        if (imageIds == null || !imageIds.Any())
            return;

        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null)
            {
                var imagesToDelete = product.ProductImages.Where(pi => imageIds.Contains(pi.Id)).ToList();
                
                foreach (var image in imagesToDelete)
                {
                    // Delete physical file
                    await _fileUploadService.DeleteProductImageAsync(image.ImageUrl);
                    
                    // Remove from product
                    product.ProductImages.Remove(image);
                }

                // Update main image URL if needed
                if (product.ProductImages.Any())
                {
                    var mainImage = product.ProductImages.FirstOrDefault(pi => pi.IsMain) ?? product.ProductImages.First();
                    product.ImageUrl = mainImage.ImageUrl;
                }
                else
                {
                    product.ImageUrl = null;
                }

                await _productService.UpdateProductAsync(product);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting images: {ex.Message}";
        }
    }

    /// <summary>
    /// Set main product image
    /// </summary>
    private async Task SetMainImageAsync(int productId, int imageId)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null)
            {
                // Reset all images to not main
                foreach (var image in product.ProductImages)
                {
                    image.IsMain = false;
                }

                // Set new main image
                var newMainImage = product.ProductImages.FirstOrDefault(pi => pi.Id == imageId);
                if (newMainImage != null)
                {
                    newMainImage.IsMain = true;
                    product.ImageUrl = newMainImage.ImageUrl;
                    await _productService.UpdateProductAsync(product);
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error setting main image: {ex.Message}";
        }
    }
}
