using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Models;
using ECommerce.Service.Interfaces;

namespace ECommerce.WebUI.Controllers;

/// <summary>
/// Controller for blog listing and detail pages
/// Routes: /blog, /blog/{slug}
/// </summary>
public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly IBlogPostService _blogPostService;

    public BlogController(
        ILogger<BlogController> logger,
        IBlogPostService blogPostService)
    {
        _logger = logger;
        _blogPostService = blogPostService;
    }

    /// <summary>
    /// Blog listing page with search and filtering
    /// GET: /blog
    /// </summary>
    [HttpGet]
    [Route("blog")]
    public async Task<IActionResult> Index(BlogFiltersViewModel filters)
    {
        try
        {
            var viewModel = new BlogListViewModel
            {
                Filters = filters
            };

            // Load blog posts with filters
            var (blogPosts, totalCount) = await _blogPostService.GetBlogPostsAsync(
                searchTerm: filters.SearchTerm,
                category: filters.Category,
                isPublished: true,
                isFeatured: null,
                author: filters.Author,
                tags: filters.Tag,
                sortBy: filters.SortBy,
                sortOrder: filters.SortOrder,
                page: filters.Page,
                pageSize: filters.PageSize
            );

            viewModel.Posts = new PagedBlogListViewModel
            {
                Posts = blogPosts.Select(MapToBlogCard).ToList(),
                CurrentPage = filters.Page,
                TotalItems = totalCount,
                PageSize = filters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
            };

            // Load featured posts for sidebar
            var (featuredPosts, _) = await _blogPostService.GetBlogPostsAsync(
                isPublished: true,
                isFeatured: true,
                sortBy: "publishedDate",
                sortOrder: "desc",
                page: 1,
                pageSize: 3
            );

            viewModel.FeaturedPosts = featuredPosts.Select(MapToBlogCard).ToList();

            // Load recent posts for sidebar
            var (recentPosts, _) = await _blogPostService.GetBlogPostsAsync(
                isPublished: true,
                sortBy: "publishedDate",
                sortOrder: "desc",
                page: 1,
                pageSize: 5
            );

            viewModel.RecentPosts = recentPosts
                .Where(p => !viewModel.Posts.Posts.Any(post => post.Id == p.Id)) // Exclude posts already in main list
                .Select(MapToBlogCard)
                .ToList();

            // Load popular tags
            var allPosts = await _blogPostService.GetPublishedBlogPostsAsync();
            viewModel.PopularTags = allPosts
                .Where(p => !string.IsNullOrEmpty(p.Tags))
                .SelectMany(p => p.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Select(g => g.Key)
                .ToList();

            // Set categories for ViewBag
            var categories = allPosts
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();
            ViewBag.Categories = categories;

            // SEO
            ViewData["Title"] = "Blog - Latest News & Articles";
            ViewData["MetaDescription"] = "Read our latest blog posts about products, industry news, and helpful tips.";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blog index");
            return View(new BlogListViewModel());
        }
    }

    /// <summary>
    /// Individual blog post detail page
    /// GET: /blog/{slug}
    /// </summary>
    [HttpGet]
    [Route("blog/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            // Find blog post by slug
            var blogPost = await _blogPostService.GetBlogPostBySlugAsync(slug);
            if (blogPost == null || !blogPost.IsPublished)
            {
                return NotFound();
            }

            // Load related posts by category or tags
            var (allPosts, _) = await _blogPostService.GetBlogPostsAsync(
                category: blogPost.Category,
                isPublished: true,
                sortBy: "publishedDate",
                sortOrder: "desc",
                page: 1,
                pageSize: 10
            );

            var relatedPosts = allPosts
                .Where(p => p.Id != blogPost.Id) // Exclude current post
                .Take(3)
                .ToList();

            var viewModel = new BlogDetailViewModel
            {
                Post = blogPost,
                RelatedPosts = relatedPosts,
                Tags = !string.IsNullOrEmpty(blogPost.Tags)
                    ? blogPost.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
                    : new List<string>(),
                MetaTitle = $"{blogPost.Title} - Blog",
                MetaDescription = blogPost.MetaDescription ?? blogPost.Summary ?? 
                    (blogPost.Content.Length > 160 ? blogPost.Content.Substring(0, 157) + "..." : blogPost.Content),
                CanonicalUrl = Url.Action("Details", "Blog", new { slug }, Request.Scheme) ?? ""
            };

            // Increment view count
            try
            {
                await _blogPostService.IncrementViewCountAsync(blogPost.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not increment view count for blog post {BlogPostId}", blogPost.Id);
            }

            // SEO and OpenGraph
            ViewData["Title"] = viewModel.MetaTitle;
            ViewData["MetaDescription"] = viewModel.MetaDescription;
            ViewData["CanonicalUrl"] = viewModel.CanonicalUrl;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blog post: {Slug}", slug);
            return NotFound();
        }
    }

    /// <summary>
    /// Search blog posts (AJAX endpoint)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string? q, int page = 1)
    {
        try
        {
            if (string.IsNullOrEmpty(q))
            {
                return Json(new { posts = new List<object>(), totalCount = 0 });
            }

            var (blogPosts, totalCount) = await _blogPostService.GetBlogPostsAsync(
                searchTerm: q,
                isPublished: true,
                sortBy: "publishedDate",
                sortOrder: "desc",
                page: page,
                pageSize: 6
            );

            var results = blogPosts.Select(p => new
            {
                id = p.Id,
                title = p.Title,
                excerpt = p.Summary ?? (p.Content.Length > 150 ? p.Content.Substring(0, 147) + "..." : p.Content),
                author = p.Author ?? "Admin",
                publishedDate = (p.PublishedDate ?? p.CreatedDate).ToString("MMM dd, yyyy"),
                imageUrl = p.ImageUrl ?? "/images/blog-placeholder.jpg",
                url = Url.Action("Details", "Blog", new { slug = p.Slug })
            }).ToList();

            return Json(new { posts = results, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching blog posts: {Query}", q);
            return Json(new { posts = new List<object>(), totalCount = 0 });
        }
    }

    /// <summary>
    /// Get posts by tag
    /// GET: /blog/tag/{tag}
    /// </summary>
    [HttpGet]
    [Route("blog/tag/{tag}")]
    public async Task<IActionResult> Tag(string tag, int page = 1)
    {
        var filters = new BlogFiltersViewModel
        {
            Tag = tag,
            Page = page
        };

        ViewData["Title"] = $"Posts tagged with '{tag}'";
        return await Index(filters);
    }

    /// <summary>
    /// Get posts by category
    /// GET: /blog/category/{category}
    /// </summary>
    [HttpGet]
    [Route("blog/category/{category}")]
    public async Task<IActionResult> Category(string category, int page = 1)
    {
        var filters = new BlogFiltersViewModel
        {
            Category = category,
            Page = page
        };

        ViewData["Title"] = $"Posts in '{category}' category";
        return await Index(filters);
    }

    /// <summary>
    /// Helper method to map BlogPost entity to BlogCardViewModel
    /// </summary>
    private BlogCardViewModel MapToBlogCard(ECommerce.Core.Entities.BlogPost blogPost)
    {
        return new BlogCardViewModel
        {
            Id = blogPost.Id,
            Title = blogPost.Title,
            Slug = blogPost.Slug,
            Excerpt = blogPost.Summary ?? (blogPost.Content.Length > 150
                ? blogPost.Content.Substring(0, 147) + "..."
                : blogPost.Content),
            FeaturedImageUrl = blogPost.ImageUrl ?? "/images/blog-placeholder.jpg",
            Author = blogPost.Author ?? "Admin",
            Category = blogPost.Category ?? "General",
            Tags = !string.IsNullOrEmpty(blogPost.Tags)
                ? blogPost.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
                : new List<string>(),
            PublishedDate = blogPost.PublishedDate ?? blogPost.CreatedDate,
            ViewCount = blogPost.ViewCount,
            IsFeatured = blogPost.IsFeatured
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
