using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ECommerce.WebUI.Areas.Admin.Models;

public class SliderListViewModel
{
    public IEnumerable<SliderItemViewModel> Sliders { get; set; } = new List<SliderItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "displayOrder";
    public string SortOrder { get; set; } = "asc";
    public SliderStatisticsViewModel Statistics { get; set; } = new();

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || IsActive.HasValue;
}

public class SliderItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public string StatusCssClass => IsActive ? "success" : "secondary";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string TruncatedDescription => Description?.Length > 100 
        ? Description[..100] + "..." 
        : Description ?? "";
}

public class SliderDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public string StatusCssClass => IsActive ? "success" : "secondary";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
}

public class SliderCreateViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? LinkUrl { get; set; }

    [Range(1, 999, ErrorMessage = "Display order must be between 1 and 999")]
    public int DisplayOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public IFormFile? ImageFile { get; set; }
    public string? ImagePath { get; set; }
}

public class SliderEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? LinkUrl { get; set; }

    [Range(1, 999, ErrorMessage = "Display order must be between 1 and 999")]
    public int DisplayOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public IFormFile? ImageFile { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class SliderDeleteViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool CanBeHardDeleted { get; set; }

    [Required(ErrorMessage = "Please provide a reason for deletion")]
    public string Reason { get; set; } = string.Empty;

    public bool HardDelete { get; set; }
}

public class SliderStatisticsViewModel
{
    public int TotalSliders { get; set; }
    public int ActiveSliders { get; set; }
    public int InactiveSliders { get; set; }
    public DateTime? LastUpdated { get; set; }
    public double TotalImageSizeMB { get; set; }
}
