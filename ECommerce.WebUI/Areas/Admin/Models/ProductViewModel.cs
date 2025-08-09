using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Models;

/// <summary>
/// ViewModel for Product create and edit operations
/// </summary>
public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    [Display(Name = "Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Discount price must be between $0 and $999,999.99")]
    [Display(Name = "Discount Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal? DiscountPrice { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be 0 or greater")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Category")]
    public string? CategoryName { get; set; }

    [Display(Name = "Main Image URL")]
    [StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Product Images")]
    public List<IFormFile> ImageFiles { get; set; } = new();

    [Display(Name = "Images to Delete")]
    public List<int> ImagesToDelete { get; set; } = new();

    [Display(Name = "Main Image ID")]
    public int? MainImageId { get; set; }

    [Display(Name = "SKU")]
    [StringLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
    public string? SKU { get; set; }

    [Display(Name = "Brand")]
    [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
    public string? Brand { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Featured")]
    public bool IsFeatured { get; set; } = false;

    [Display(Name = "View Count")]
    public int ViewCount { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }

    [Display(Name = "Updated Date")]
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties for display
    public List<SelectListItem> Categories { get; set; } = new();
    public List<ProductImageViewModel> ProductImages { get; set; } = new();

    // Computed properties
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal EffectivePrice => DiscountPrice ?? Price;
    public bool IsLowStock => StockQuantity <= 10;
    public bool IsOutOfStock => StockQuantity == 0;
}

/// <summary>
/// ViewModel for Product listing with pagination and filtering
/// </summary>
public class ProductListViewModel
{
    public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    
    // Pagination properties
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 10;
    
    // Search and filter properties
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }
    
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }
    
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "name";
    
    [Display(Name = "Sort Order")]
    public string SortOrder { get; set; } = "asc";
    
    // Available options for dropdowns
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> SortOptions { get; set; } = new()
    {
        new SelectListItem { Value = "name", Text = "Name" },
        new SelectListItem { Value = "price", Text = "Price" },
        new SelectListItem { Value = "stock", Text = "Stock" },
        new SelectListItem { Value = "category", Text = "Category" },
        new SelectListItem { Value = "created", Text = "Created Date" }
    };
    
    // Computed properties
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || CategoryId.HasValue;
}

/// <summary>
/// ViewModel for Product delete confirmation
/// </summary>
public class ProductDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? SKU { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int OrderCount { get; set; }
    
    // Computed properties
    public bool HasOrders => OrderCount > 0;
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal EffectivePrice => DiscountPrice ?? Price;
}

/// <summary>
/// ViewModel for Product details view
/// </summary>
public class ProductDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? SKU { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    
    public List<ProductImageViewModel> ProductImages { get; set; } = new();
    
    // Computed properties
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal EffectivePrice => DiscountPrice ?? Price;
    public decimal DiscountPercentage => HasDiscount ? Math.Round((1 - (DiscountPrice!.Value / Price)) * 100, 1) : 0;
    public bool IsLowStock => StockQuantity <= 10;
    public bool IsOutOfStock => StockQuantity == 0;
}

/// <summary>
/// ViewModel for Product images
/// </summary>
public class ProductImageViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}
