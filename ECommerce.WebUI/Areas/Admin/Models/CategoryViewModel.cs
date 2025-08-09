using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Models;

/// <summary>
/// ViewModel for Category create and edit operations
/// </summary>
public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Image URL")]
    [StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Display Order")]
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }

    [Display(Name = "Updated Date")]
    public DateTime? UpdatedDate { get; set; }

    // For edit operations
    public int ProductCount { get; set; }
}

/// <summary>
/// ViewModel for Category listing with pagination
/// </summary>
public class CategoryListViewModel
{
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    
    // Pagination properties
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 10;
    
    // Search properties
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }
    
    // Computed properties
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
}

/// <summary>
/// ViewModel for Category delete confirmation
/// </summary>
public class CategoryDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public bool HasProducts => ProductCount > 0;
    public DateTime CreatedDate { get; set; }
}
