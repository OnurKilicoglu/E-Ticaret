using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for BlogPost management in Admin area
/// </summary>
[Area("Admin")]
public class BlogPostController : Controller
{
    private readonly IBlogPostService _blogPostService;
    private readonly ILogger<BlogPostController> _logger;

    public BlogPostController(IBlogPostService blogPostService, ILogger<BlogPostController> logger)
    {
        _blogPostService = blogPostService;
        _logger = logger;
    }

    /// <summary>
    /// Display list of blog posts with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? category, bool? isPublished, 
        bool? isFeatured, string? author, string? tags, DateTime? fromDate, DateTime? toDate,
        string sortBy = "publishedDate", string sortOrder = "desc", int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Blog Post Management";

        try
        {
            var (blogPosts, totalCount) = await _blogPostService.GetBlogPostsAsync(
                searchTerm, category, isPublished, isFeatured, author, tags, 
                fromDate, toDate, sortBy, sortOrder, page, pageSize);

            var statistics = await _blogPostService.GetBlogPostStatisticsAsync();
            var categories = await _blogPostService.GetCategoriesAsync();
            var authors = await _blogPostService.GetAuthorsAsync();
            var tagOptions = await _blogPostService.GetTagsAsync();

            var viewModel = new BlogPostListViewModel
            {
                BlogPosts = blogPosts.Select(bp => new BlogPostItemViewModel
                {
                    Id = bp.Id,
                    Title = bp.Title,
                    Summary = bp.Summary,
                    Author = bp.Author,
                    ImageUrl = bp.ImageUrl,
                    Category = bp.Category,
                    Tags = bp.Tags,
                    IsPublished = bp.IsPublished,
                    IsFeatured = bp.IsFeatured,
                    CreatedDate = bp.CreatedDate,
                    PublishedDate = bp.PublishedDate,
                    UpdatedDate = bp.UpdatedDate,
                    ViewCount = bp.ViewCount,
                    CommentCount = bp.Comments.Count,
                    Slug = bp.Slug
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                Category = category,
                IsPublished = isPublished,
                IsFeatured = isFeatured,
                Author = author,
                Tags = tags,
                FromDate = fromDate,
                ToDate = toDate,
                SortBy = sortBy,
                SortOrder = sortOrder,
                CategoryOptions = categories.Select(c => new SelectListItem { Value = c, Text = c }).ToList(),
                AuthorOptions = authors.Select(a => new SelectListItem { Value = a, Text = a }).ToList(),
                TagOptions = tagOptions.ToList(),
                Statistics = new BlogPostStatisticsViewModel
                {
                    TotalPosts = statistics.TotalPosts,
                    PublishedPosts = statistics.PublishedPosts,
                    DraftPosts = statistics.DraftPosts,
                    FeaturedPosts = statistics.FeaturedPosts,
                    TotalViews = statistics.TotalViews,
                    TotalComments = statistics.TotalComments,
                    CategoriesCount = statistics.CategoriesCount,
                    TagsCount = statistics.TagsCount,
                    LastPublished = statistics.LastPublished,
                    MostViewedPostTitle = statistics.MostViewedPost?.Title,
                    MostViewedPostViews = statistics.MostViewedPost?.ViewCount ?? 0,
                    LatestPostTitle = statistics.LatestPost?.Title,
                    LatestPostDate = statistics.LatestPost?.CreatedDate
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading blog posts: {ex.Message}";
            return View(new BlogPostListViewModel());
        }
    }

    /// <summary>
    /// Display detailed blog post information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Blog Post Details";

        try
        {
            var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                TempData["ErrorMessage"] = "Blog post not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BlogPostDetailViewModel
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Content = blogPost.Content,
                Author = blogPost.Author,
                ImageUrl = blogPost.ImageUrl,
                Summary = blogPost.Summary,
                Slug = blogPost.Slug,
                MetaDescription = blogPost.MetaDescription,
                MetaKeywords = blogPost.MetaKeywords,
                IsPublished = blogPost.IsPublished,
                IsFeatured = blogPost.IsFeatured,
                PublishedDate = blogPost.PublishedDate,
                ViewCount = blogPost.ViewCount,
                Category = blogPost.Category,
                Tags = blogPost.Tags,
                CreatedDate = blogPost.CreatedDate,
                UpdatedDate = blogPost.UpdatedDate,
                CommentCount = blogPost.Comments.Count
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading blog post details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display blog post creation form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create New Blog Post";

        try
        {
            var categories = await _blogPostService.GetCategoriesAsync();
            var authors = await _blogPostService.GetAuthorsAsync();
            var tags = await _blogPostService.GetTagsAsync();

            var viewModel = new BlogPostCreateViewModel
            {
                Author = User.Identity?.Name ?? "Admin", // Default to current user
                CategoryOptions = categories.Select(c => new SelectListItem { Value = c, Text = c }).ToList(),
                AuthorOptions = authors.Select(a => new SelectListItem { Value = a, Text = a }).ToList(),
                TagOptions = tags.ToList()
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
    /// Process blog post creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostCreateViewModel model)
    {
        ViewData["Title"] = "Create New Blog Post";

        if (ModelState.IsValid)
        {
            try
            {
                var blogPost = new BlogPost
                {
                    Title = model.Title,
                    Content = model.Content,
                    Author = model.Author,
                    Summary = model.Summary,
                    Slug = model.Slug,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    PublishedDate = model.PublishedDate,
                    Category = model.Category,
                    Tags = model.Tags
                };

                var createdBlogPost = await _blogPostService.CreateBlogPostAsync(blogPost, model.ImageFile);

                if (createdBlogPost != null)
                {
                    TempData["SuccessMessage"] = $"Blog post '{model.Title}' has been created successfully.";
                    return RedirectToAction(nameof(Details), new { id = createdBlogPost.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create blog post.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating blog post: {ex.Message}";
            }
        }

        // Reload dropdown options if model is invalid
        try
        {
            var categories = await _blogPostService.GetCategoriesAsync();
            var authors = await _blogPostService.GetAuthorsAsync();
            var tags = await _blogPostService.GetTagsAsync();

            model.CategoryOptions = categories.Select(c => new SelectListItem { Value = c, Text = c }).ToList();
            model.AuthorOptions = authors.Select(a => new SelectListItem { Value = a, Text = a }).ToList();
            model.TagOptions = tags.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading dropdown options");
        }

        return View(model);
    }

    /// <summary>
    /// Display blog post edit form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Blog Post";

        try
        {
            var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                TempData["ErrorMessage"] = "Blog post not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _blogPostService.GetCategoriesAsync();
            var authors = await _blogPostService.GetAuthorsAsync();
            var tags = await _blogPostService.GetTagsAsync();

            var viewModel = new BlogPostEditViewModel
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Content = blogPost.Content,
                Author = blogPost.Author,
                Summary = blogPost.Summary,
                Slug = blogPost.Slug,
                MetaDescription = blogPost.MetaDescription,
                MetaKeywords = blogPost.MetaKeywords,
                IsPublished = blogPost.IsPublished,
                IsFeatured = blogPost.IsFeatured,
                PublishedDate = blogPost.PublishedDate,
                Category = blogPost.Category,
                Tags = blogPost.Tags,
                ImageUrl = blogPost.ImageUrl,
                CreatedDate = blogPost.CreatedDate,
                ViewCount = blogPost.ViewCount,
                CategoryOptions = categories.Select(c => new SelectListItem { Value = c, Text = c }).ToList(),
                AuthorOptions = authors.Select(a => new SelectListItem { Value = a, Text = a }).ToList(),
                TagOptions = tags.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading blog post: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process blog post update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPostEditViewModel model)
    {
        ViewData["Title"] = "Edit Blog Post";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid blog post ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                var blogPost = new BlogPost
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content,
                    Author = model.Author,
                    Summary = model.Summary,
                    Slug = model.Slug,
                    MetaDescription = model.MetaDescription,
                    MetaKeywords = model.MetaKeywords,
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    PublishedDate = model.PublishedDate,
                    Category = model.Category,
                    Tags = model.Tags
                };

                var success = await _blogPostService.UpdateBlogPostAsync(blogPost, model.ImageFile);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Blog post '{model.Title}' has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update blog post.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating blog post: {ex.Message}";
            }
        }

        // Reload dropdown options if model is invalid
        try
        {
            var categories = await _blogPostService.GetCategoriesAsync();
            var authors = await _blogPostService.GetAuthorsAsync();
            var tags = await _blogPostService.GetTagsAsync();

            model.CategoryOptions = categories.Select(c => new SelectListItem { Value = c, Text = c }).ToList();
            model.AuthorOptions = authors.Select(a => new SelectListItem { Value = a, Text = a }).ToList();
            model.TagOptions = tags.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading dropdown options");
        }

        return View(model);
    }

    /// <summary>
    /// Display blog post deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Blog Post";

        try
        {
            var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                TempData["ErrorMessage"] = "Blog post not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BlogPostDeleteViewModel
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Summary = blogPost.Summary,
                Author = blogPost.Author,
                ImageUrl = blogPost.ImageUrl,
                Category = blogPost.Category,
                Tags = blogPost.Tags,
                IsPublished = blogPost.IsPublished,
                IsFeatured = blogPost.IsFeatured,
                CreatedDate = blogPost.CreatedDate,
                PublishedDate = blogPost.PublishedDate,
                ViewCount = blogPost.ViewCount,
                CommentCount = blogPost.Comments.Count,
                CanBeHardDeleted = blogPost.Comments.Count == 0 // Can hard delete if no comments
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading blog post: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process blog post deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, BlogPostDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                bool success;
                
                if (model.HardDelete && model.CanBeHardDeleted)
                {
                    success = await _blogPostService.HardDeleteBlogPostAsync(id);
                    TempData["SuccessMessage"] = $"Blog post '{model.Title}' has been permanently deleted.";
                }
                else
                {
                    success = await _blogPostService.DeleteBlogPostAsync(id);
                    TempData["SuccessMessage"] = $"Blog post '{model.Title}' has been deactivated.";
                }

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete blog post.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting blog post: {ex.Message}";
            return View("Delete", model);
        }
    }

    /// <summary>
    /// AJAX endpoint for quick published status toggle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TogglePublished(int id)
    {
        try
        {
            var success = await _blogPostService.TogglePublishedStatusAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Published status updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update published status" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for quick featured status toggle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleFeatured(int id)
    {
        try
        {
            var success = await _blogPostService.ToggleFeaturedStatusAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Featured status updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update featured status" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for generating slug from title
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GenerateSlug(string title, int? id = null)
    {
        try
        {
            var slug = await _blogPostService.GenerateSlugAsync(title, id);
            return Json(new { success = true, slug });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for checking slug uniqueness
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckSlug(string slug, int? id = null)
    {
        try
        {
            var isUnique = await _blogPostService.IsSlugUniqueAsync(slug, id);
            return Json(new { available = isUnique });
        }
        catch
        {
            return Json(new { available = false });
        }
    }

    /// <summary>
    /// Get tag suggestions for autocomplete
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTagSuggestions()
    {
        try
        {
            var tags = await _blogPostService.GetTagsAsync();
            return Json(tags.Take(20)); // Limit to 20 suggestions
        }
        catch
        {
            return Json(new string[0]);
        }
    }

    /// <summary>
    /// Get category suggestions for autocomplete
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategorySuggestions()
    {
        try
        {
            var categories = await _blogPostService.GetCategoriesAsync();
            return Json(categories.Take(20)); // Limit to 20 suggestions
        }
        catch
        {
            return Json(new string[0]);
        }
    }
}
