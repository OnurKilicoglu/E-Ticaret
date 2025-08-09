using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for BlogPost entity operations
/// </summary>
public interface IBlogPostService
{
    /// <summary>
    /// Get all blog posts with filtering and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for title, content, or author</param>
    /// <param name="category">Filter by category</param>
    /// <param name="isPublished">Filter by published status</param>
    /// <param name="isFeatured">Filter by featured status</param>
    /// <param name="author">Filter by author</param>
    /// <param name="tags">Filter by tags (comma-separated)</param>
    /// <param name="fromDate">Filter posts published from this date</param>
    /// <param name="toDate">Filter posts published to this date</param>
    /// <param name="sortBy">Sort field (publishedDate, viewCount, title)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing blog posts and total count</returns>
    Task<(IEnumerable<BlogPost> BlogPosts, int TotalCount)> GetBlogPostsAsync(
        string? searchTerm = null,
        string? category = null,
        bool? isPublished = null,
        bool? isFeatured = null,
        string? author = null,
        string? tags = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "publishedDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get blog post by ID
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>Blog post entity or null if not found</returns>
    Task<BlogPost?> GetBlogPostByIdAsync(int id);

    /// <summary>
    /// Get blog post by slug for frontend
    /// </summary>
    /// <param name="slug">Blog post slug</param>
    /// <returns>Published blog post or null if not found</returns>
    Task<BlogPost?> GetBlogPostBySlugAsync(string slug);

    /// <summary>
    /// Get published blog posts for frontend
    /// </summary>
    /// <param name="category">Optional category filter</param>
    /// <param name="featured">Filter for featured posts only</param>
    /// <param name="pageSize">Number of posts to return</param>
    /// <returns>List of published blog posts</returns>
    Task<IEnumerable<BlogPost>> GetPublishedBlogPostsAsync(string? category = null, bool? featured = null, int pageSize = 10);

    /// <summary>
    /// Create new blog post with image upload
    /// </summary>
    /// <param name="blogPost">Blog post entity</param>
    /// <param name="imageFile">Feature image file to upload</param>
    /// <returns>Created blog post or null if failed</returns>
    Task<BlogPost?> CreateBlogPostAsync(BlogPost blogPost, IFormFile? imageFile = null);

    /// <summary>
    /// Update existing blog post
    /// </summary>
    /// <param name="blogPost">Updated blog post entity</param>
    /// <param name="imageFile">New feature image file (optional)</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateBlogPostAsync(BlogPost blogPost, IFormFile? imageFile = null);

    /// <summary>
    /// Delete blog post (soft delete)
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteBlogPostAsync(int id);

    /// <summary>
    /// Hard delete blog post and remove image file
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>True if successful</returns>
    Task<bool> HardDeleteBlogPostAsync(int id);

    /// <summary>
    /// Toggle blog post published status
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>True if successful</returns>
    Task<bool> TogglePublishedStatusAsync(int id);

    /// <summary>
    /// Toggle blog post featured status
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>True if successful</returns>
    Task<bool> ToggleFeaturedStatusAsync(int id);

    /// <summary>
    /// Increment view count for blog post
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>True if successful</returns>
    Task<bool> IncrementViewCountAsync(int id);

    /// <summary>
    /// Generate unique slug from title
    /// </summary>
    /// <param name="title">Blog post title</param>
    /// <param name="existingId">Existing blog post ID to exclude from uniqueness check</param>
    /// <returns>Unique slug</returns>
    Task<string> GenerateSlugAsync(string title, int? existingId = null);

    /// <summary>
    /// Check if slug is unique
    /// </summary>
    /// <param name="slug">Slug to check</param>
    /// <param name="existingId">Existing blog post ID to exclude</param>
    /// <returns>True if unique</returns>
    Task<bool> IsSlugUniqueAsync(string slug, int? existingId = null);

    /// <summary>
    /// Get all categories used in blog posts
    /// </summary>
    /// <returns>List of unique categories</returns>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// Get all tags used in blog posts
    /// </summary>
    /// <returns>List of unique tags</returns>
    Task<IEnumerable<string>> GetTagsAsync();

    /// <summary>
    /// Get all authors
    /// </summary>
    /// <returns>List of unique authors</returns>
    Task<IEnumerable<string>> GetAuthorsAsync();

    /// <summary>
    /// Get blog post statistics
    /// </summary>
    /// <returns>Statistics about blog posts</returns>
    Task<BlogPostStatistics> GetBlogPostStatisticsAsync();

    /// <summary>
    /// Get related blog posts based on category and tags
    /// </summary>
    /// <param name="blogPostId">Current blog post ID</param>
    /// <param name="count">Number of related posts to return</param>
    /// <returns>List of related blog posts</returns>
    Task<IEnumerable<BlogPost>> GetRelatedBlogPostsAsync(int blogPostId, int count = 5);

    /// <summary>
    /// Check if blog post exists
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <returns>True if exists</returns>
    Task<bool> BlogPostExistsAsync(int id);
}

/// <summary>
/// Blog post statistics model
/// </summary>
public class BlogPostStatistics
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
    public BlogPost? MostViewedPost { get; set; }
    public BlogPost? LatestPost { get; set; }
}

