using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Models;

public class BlogPostListViewModel
{
    public IEnumerable<BlogPostItemViewModel> BlogPosts { get; set; } = new List<BlogPostItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsFeatured { get; set; }
    public string? Author { get; set; }
    public string? Tags { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "publishedDate";
    public string SortOrder { get; set; } = "desc";
    public BlogPostStatisticsViewModel Statistics { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<SelectListItem> AuthorOptions { get; set; } = new();
    public List<string> TagOptions { get; set; } = new();

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || !string.IsNullOrEmpty(Category) || 
                             IsPublished.HasValue || IsFeatured.HasValue || !string.IsNullOrEmpty(Author) || 
                             !string.IsNullOrEmpty(Tags) || FromDate.HasValue || ToDate.HasValue;
}

public class BlogPostItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public string? Slug { get; set; }

    public string StatusCssClass => IsPublished ? "success" : "warning";
    public string StatusDisplayName => IsPublished ? "Published" : "Draft";
    public string FeaturedCssClass => IsFeatured ? "primary" : "secondary";
    public string FeaturedDisplayName => IsFeatured ? "Featured" : "Regular";
    public string TruncatedSummary => Summary?.Length > 150 
        ? Summary[..150] + "..." 
        : Summary ?? "";
    public string TruncatedTitle => Title.Length > 50 
        ? Title[..50] + "..." 
        : Title;
    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToList() ?? new List<string>();
}

public class BlogPostDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Summary { get; set; }
    public string? Slug { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int ViewCount { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int CommentCount { get; set; }

    public string StatusCssClass => IsPublished ? "success" : "warning";
    public string StatusDisplayName => IsPublished ? "Published" : "Draft";
    public string FeaturedCssClass => IsFeatured ? "primary" : "secondary";
    public string FeaturedDisplayName => IsFeatured ? "Featured" : "Regular";
    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToList() ?? new List<string>();
    public string ReadingTime => CalculateReadingTime(Content);

    private static string CalculateReadingTime(string content)
    {
        if (string.IsNullOrEmpty(content)) return "0 min";
        
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var minutes = Math.Max(1, wordCount / 200); // Assuming 200 words per minute
        return $"{minutes} min read";
    }
}

public class BlogPostCreateViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required")]
    [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
    public string Author { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    public string? Summary { get; set; }

    [StringLength(200, ErrorMessage = "Slug cannot exceed 200 characters")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
    public string? Slug { get; set; }

    [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
    public string? MetaDescription { get; set; }

    [StringLength(200, ErrorMessage = "Meta keywords cannot exceed 200 characters")]
    public string? MetaKeywords { get; set; }

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    [DataType(DataType.DateTime)]
    public DateTime? PublishedDate { get; set; }

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }

    public IFormFile? ImageFile { get; set; }
    public string? ImageUrl { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<string> TagOptions { get; set; } = new();
    public List<SelectListItem> AuthorOptions { get; set; } = new();
}

public class BlogPostEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required")]
    [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
    public string Author { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    public string? Summary { get; set; }

    [StringLength(200, ErrorMessage = "Slug cannot exceed 200 characters")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
    public string? Slug { get; set; }

    [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
    public string? MetaDescription { get; set; }

    [StringLength(200, ErrorMessage = "Meta keywords cannot exceed 200 characters")]
    public string? MetaKeywords { get; set; }

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    [DataType(DataType.DateTime)]
    public DateTime? PublishedDate { get; set; }

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }

    public IFormFile? ImageFile { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ViewCount { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<string> TagOptions { get; set; } = new();
    public List<SelectListItem> AuthorOptions { get; set; } = new();
}

public class BlogPostDeleteViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public bool CanBeHardDeleted { get; set; }

    [Required(ErrorMessage = "Please provide a reason for deletion")]
    public string Reason { get; set; } = string.Empty;

    public bool HardDelete { get; set; }

    public string StatusCssClass => IsPublished ? "success" : "warning";
    public string StatusDisplayName => IsPublished ? "Published" : "Draft";
    public string FeaturedCssClass => IsFeatured ? "primary" : "secondary";
    public string FeaturedDisplayName => IsFeatured ? "Featured" : "Regular";
    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToList() ?? new List<string>();
}

public class BlogPostStatisticsViewModel
{
    public int TotalPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
    public int FeaturedPosts { get; set; }
    public int TotalViews { get; set; }
    public int TotalComments { get; set; }
    public int CategoriesCount { get; set; }
    public int TagsCount { get; set; }
    public DateTime? LastPublished { get; set; }
    public string? MostViewedPostTitle { get; set; }
    public int MostViewedPostViews { get; set; }
    public string? LatestPostTitle { get; set; }
    public DateTime? LatestPostDate { get; set; }
}

