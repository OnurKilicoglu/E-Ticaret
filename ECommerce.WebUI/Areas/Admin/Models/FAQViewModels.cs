using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Models;

#region FAQ ViewModels

public class FAQListViewModel
{
    public IEnumerable<FAQItemViewModel> FAQs { get; set; } = new List<FAQItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public string? Tags { get; set; }
    public string? Author { get; set; }
    public string SortBy { get; set; } = "displayOrder";
    public string SortOrder { get; set; } = "asc";
    public FAQStatisticsViewModel Statistics { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<string> TagOptions { get; set; } = new();
    public List<SelectListItem> AuthorOptions { get; set; } = new();

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || CategoryId.HasValue || 
                             IsActive.HasValue || !string.IsNullOrEmpty(Tags) || !string.IsNullOrEmpty(Author);
}

public class FAQItemViewModel
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public string? Tags { get; set; }
    public string? Author { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public string StatusCssClass => IsActive ? "success" : "warning";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string TruncatedQuestion => Question.Length > 80 ? Question[..80] + "..." : Question;
    public string TruncatedAnswer => Answer.Length > 120 ? Answer[..120] + "..." : Answer;
    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToList() ?? new List<string>();
    public int HelpfulnessScore => HelpfulCount - NotHelpfulCount;
    public string HelpfulnessIcon => HelpfulnessScore > 0 ? "bi-hand-thumbs-up-fill text-success" :
                                     HelpfulnessScore < 0 ? "bi-hand-thumbs-down-fill text-danger" :
                                     "bi-dash-circle text-muted";
}

public class FAQDetailViewModel
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public string? Tags { get; set; }
    public string? Author { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public string StatusCssClass => IsActive ? "success" : "warning";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToList() ?? new List<string>();
    public int HelpfulnessScore => HelpfulCount - NotHelpfulCount;
    public double HelpfulPercentage => (HelpfulCount + NotHelpfulCount) > 0 
        ? (double)HelpfulCount / (HelpfulCount + NotHelpfulCount) * 100 
        : 0;
}

public class FAQCreateViewModel
{
    [Required(ErrorMessage = "Question is required")]
    [StringLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer is required")]
    public string Answer { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [StringLength(200, ErrorMessage = "Tags cannot exceed 200 characters")]
    public string? Tags { get; set; }

    [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
    public string? Author { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<string> TagOptions { get; set; } = new();
    public List<SelectListItem> AuthorOptions { get; set; } = new();
}

public class FAQEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Question is required")]
    [StringLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer is required")]
    public string Answer { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    [StringLength(200, ErrorMessage = "Tags cannot exceed 200 characters")]
    public string? Tags { get; set; }

    [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
    public string? Author { get; set; }

    public DateTime CreatedDate { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<string> TagOptions { get; set; } = new();
    public List<SelectListItem> AuthorOptions { get; set; } = new();
}

public class FAQDeleteViewModel
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedDate { get; set; }

    [Required(ErrorMessage = "Please provide a reason for deletion")]
    public string Reason { get; set; } = string.Empty;

    public bool HardDelete { get; set; }

    public string StatusCssClass => IsActive ? "success" : "warning";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToList() ?? new List<string>();
    public string TruncatedAnswer => Answer.Length > 200 ? Answer[..200] + "..." : Answer;
}

#endregion

#region FAQ Category ViewModels

public class FAQCategoryListViewModel
{
    public IEnumerable<FAQCategoryItemViewModel> Categories { get; set; } = new List<FAQCategoryItemViewModel>();
    public FAQCategoryStatisticsViewModel Statistics { get; set; } = new();
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "displayOrder";
    public string SortOrder { get; set; } = "asc";
    public List<string> AvailableIcons { get; set; } = new();
    
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || IsActive.HasValue;
}

public class FAQCategoryItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int FAQCount { get; set; }
    public int ActiveFAQCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public string StatusCssClass => IsActive ? "success" : "warning";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string TruncatedDescription => Description?.Length > 100 
        ? Description[..100] + "..." 
        : Description ?? "";
    public string DisplayIcon => !string.IsNullOrEmpty(Icon) ? Icon : "bi bi-tag";
    public string EfficiencyRate => FAQCount > 0 ? $"{(double)ActiveFAQCount / FAQCount * 100:F1}%" : "0%";
}

public class FAQCategoryDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int FAQCount { get; set; }
    public int ActiveFAQCount { get; set; }
    public int TotalViews { get; set; }
    public int TotalHelpfulVotes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public IEnumerable<FAQItemViewModel> RecentFAQs { get; set; } = new List<FAQItemViewModel>();
    public IEnumerable<FAQItemViewModel> PopularFAQs { get; set; } = new List<FAQItemViewModel>();

    public string StatusCssClass => IsActive ? "success" : "warning";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string DisplayIcon => !string.IsNullOrEmpty(Icon) ? Icon : "bi bi-tag";
    public double EfficiencyRate => FAQCount > 0 ? (double)ActiveFAQCount / FAQCount * 100 : 0;
    public double AverageHelpfulness => FAQCount > 0 && TotalHelpfulVotes > 0 ? (double)TotalHelpfulVotes / FAQCount : 0;
}

public class FAQCategoryCreateViewModel
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Icon cannot exceed 100 characters")]
    [Display(Name = "Icon")]
    public string? Icon { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Icon Upload")]
    public IFormFile? IconFile { get; set; }

    public List<string> PredefinedIcons { get; set; } = new();
    public List<string> PopularIcons { get; set; } = new();
}

public class FAQCategoryEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Icon cannot exceed 100 characters")]
    [Display(Name = "Icon")]
    public string? Icon { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a positive number")]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Icon Upload")]
    public IFormFile? IconFile { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int FAQCount { get; set; }
    public int ActiveFAQCount { get; set; }
    public int TotalViews { get; set; }

    public List<string> PredefinedIcons { get; set; } = new();
    public List<string> PopularIcons { get; set; } = new();
    public string CurrentIconUrl { get; set; } = string.Empty;
}

public class FAQCategoryDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int FAQCount { get; set; }
    public int ActiveFAQCount { get; set; }
    public int TotalViews { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    [Required(ErrorMessage = "Please provide a reason for deletion")]
    [Display(Name = "Reason for Deletion")]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "Hard Delete")]
    public bool HardDelete { get; set; }

    [Display(Name = "Move FAQs to Category")]
    public int? MoveFAQsToCategoryId { get; set; }

    public List<SelectListItem> AvailableCategories { get; set; } = new();
    public IEnumerable<FAQItemViewModel> AssociatedFAQs { get; set; } = new List<FAQItemViewModel>();

    public string StatusCssClass => IsActive ? "success" : "warning";
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string DisplayIcon => !string.IsNullOrEmpty(Icon) ? Icon : "bi bi-tag";
    public string TruncatedDescription => Description?.Length > 200 ? Description[..200] + "..." : Description ?? "";
}

public class FAQCategoryStatisticsViewModel
{
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int InactiveCategories { get; set; }
    public int TotalFAQs { get; set; }
    public int CategorizedFAQs { get; set; }
    public int UncategorizedFAQs { get; set; }
    public string? MostPopularCategoryName { get; set; }
    public int MostPopularCategoryFAQCount { get; set; }
    public string? MostViewedCategoryName { get; set; }
    public int MostViewedCategoryViews { get; set; }
    public DateTime? LastUpdated { get; set; }
    public double AverageFAQsPerCategory => ActiveCategories > 0 ? (double)CategorizedFAQs / ActiveCategories : 0;
    public double CategoryUtilizationRate => TotalCategories > 0 ? (double)ActiveCategories / TotalCategories * 100 : 0;
}

public class FAQCategoryOrderUpdateViewModel
{
    public int Id { get; set; }
    public int NewOrder { get; set; }
}

public class BulkFAQCategoryOrderUpdateViewModel
{
    public List<FAQCategoryOrderUpdateViewModel> Updates { get; set; } = new();
}

#endregion

#region Statistics ViewModels

public class FAQStatisticsViewModel
{
    public int TotalFAQs { get; set; }
    public int ActiveFAQs { get; set; }
    public int InactiveFAQs { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int TotalViews { get; set; }
    public int TotalHelpfulVotes { get; set; }
    public int TotalNotHelpfulVotes { get; set; }
    public string? MostViewedFAQQuestion { get; set; }
    public int MostViewedFAQViews { get; set; }
    public string? MostHelpfulFAQQuestion { get; set; }
    public int MostHelpfulFAQScore { get; set; }
    public string? MostPopularCategoryName { get; set; }
    public int MostPopularCategoryCount { get; set; }
    public DateTime? LastUpdated { get; set; }
}

#endregion

#region Helper ViewModels

public class FAQOrderUpdateViewModel
{
    public int Id { get; set; }
    public int NewOrder { get; set; }
}

public class BulkFAQOrderUpdateViewModel
{
    public List<FAQOrderUpdateViewModel> Updates { get; set; } = new();
}

#endregion
