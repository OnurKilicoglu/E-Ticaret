using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for BlogPost entity operations
/// </summary>
public class BlogPostService : IBlogPostService
{
    private readonly ECommerceDbContext _context;
    private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<BlogPostService> _logger;

    public BlogPostService(
        ECommerceDbContext context,
        IImageUploadService imageUploadService,
        ILogger<BlogPostService> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    public async Task<(IEnumerable<BlogPost> BlogPosts, int TotalCount)> GetBlogPostsAsync(
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
        int pageSize = 20)
    {
        try
        {
            var query = _context.BlogPosts.Include(bp => bp.Comments).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(bp => bp.Title.Contains(searchTerm) || 
                                         bp.Content.Contains(searchTerm) ||
                                         bp.Author.Contains(searchTerm) ||
                                         (bp.Summary != null && bp.Summary.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(bp => bp.Category == category);
            }

            if (isPublished.HasValue)
            {
                query = query.Where(bp => bp.IsPublished == isPublished.Value);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(bp => bp.IsFeatured == isFeatured.Value);
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(bp => bp.Author.Contains(author));
            }

            if (!string.IsNullOrEmpty(tags))
            {
                var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim().ToLower());
                foreach (var tag in tagList)
                {
                    query = query.Where(bp => bp.Tags != null && bp.Tags.ToLower().Contains(tag));
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(bp => bp.PublishedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(bp => bp.PublishedDate <= toDate.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(bp => bp.Title)
                    : query.OrderBy(bp => bp.Title),
                "author" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.Author)
                    : query.OrderBy(bp => bp.Author),
                "viewcount" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.ViewCount)
                    : query.OrderBy(bp => bp.ViewCount),
                "createddate" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.CreatedDate)
                    : query.OrderBy(bp => bp.CreatedDate),
                "publisheddate" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(bp => bp.PublishedDate)
                    : query.OrderBy(bp => bp.PublishedDate),
                _ => query.OrderByDescending(bp => bp.PublishedDate ?? bp.CreatedDate)
            };

            // Apply pagination
            var blogPosts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (blogPosts, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog posts with filters");
            return (Enumerable.Empty<BlogPost>(), 0);
        }
    }

    public async Task<BlogPost?> GetBlogPostByIdAsync(int id)
    {
        try
        {
            return await _context.BlogPosts
                .Include(bp => bp.Comments)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post by ID: {Id}", id);
            return null;
        }
    }

    public async Task<BlogPost?> GetBlogPostBySlugAsync(string slug)
    {
        try
        {
            return await _context.BlogPosts
                .Include(bp => bp.Comments)
                .FirstOrDefaultAsync(bp => bp.Slug == slug && bp.IsPublished);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post by slug: {Slug}", slug);
            return null;
        }
    }

    public async Task<IEnumerable<BlogPost>> GetPublishedBlogPostsAsync(string? category = null, bool? featured = null, int pageSize = 10)
    {
        try
        {
            var query = _context.BlogPosts
                .Where(bp => bp.IsPublished && bp.PublishedDate <= DateTime.UtcNow);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(bp => bp.Category == category);
            }

            if (featured.HasValue)
            {
                query = query.Where(bp => bp.IsFeatured == featured.Value);
            }

            return await query
                .OrderByDescending(bp => bp.PublishedDate)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published blog posts");
            return Enumerable.Empty<BlogPost>();
        }
    }

    public async Task<BlogPost?> CreateBlogPostAsync(BlogPost blogPost, IFormFile? imageFile = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Handle feature image upload
            if (imageFile != null)
            {
                var imagePath = await _imageUploadService.SaveImageAsync(imageFile, "blog");
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogError("Failed to save feature image for blog post");
                    return null;
                }

                blogPost.ImageUrl = imagePath;
            }

            // Generate slug if not provided
            if (string.IsNullOrEmpty(blogPost.Slug))
            {
                blogPost.Slug = await GenerateSlugAsync(blogPost.Title);
            }
            else
            {
                // Ensure slug is unique
                if (!await IsSlugUniqueAsync(blogPost.Slug))
                {
                    blogPost.Slug = await GenerateSlugAsync(blogPost.Title);
                }
            }

            // Set published date if published
            if (blogPost.IsPublished && !blogPost.PublishedDate.HasValue)
            {
                blogPost.PublishedDate = DateTime.UtcNow;
            }

            // Set created date
            blogPost.CreatedDate = DateTime.UtcNow;

            // Clean and format tags
            if (!string.IsNullOrEmpty(blogPost.Tags))
            {
                blogPost.Tags = CleanTags(blogPost.Tags);
            }

            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Created blog post with ID: {Id}", blogPost.Id);
            return blogPost;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating blog post");

            // Clean up uploaded image if it exists
            if (!string.IsNullOrEmpty(blogPost.ImageUrl))
            {
                _imageUploadService.DeleteImage(blogPost.ImageUrl);
            }

            return null;
        }
    }

    public async Task<bool> UpdateBlogPostAsync(BlogPost blogPost, IFormFile? imageFile = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingBlogPost = await _context.BlogPosts.FindAsync(blogPost.Id);
            if (existingBlogPost == null)
            {
                _logger.LogWarning("Blog post not found for update: {Id}", blogPost.Id);
                return false;
            }

            var oldImagePath = existingBlogPost.ImageUrl;

            // Handle new feature image upload
            if (imageFile != null)
            {
                var imagePath = await _imageUploadService.SaveImageAsync(imageFile, "blog");
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogError("Failed to save new feature image for blog post");
                    return false;
                }

                blogPost.ImageUrl = imagePath;
            }
            else
            {
                // Keep existing image
                blogPost.ImageUrl = existingBlogPost.ImageUrl;
            }

            // Ensure slug is unique (excluding current post)
            if (!await IsSlugUniqueAsync(blogPost.Slug, blogPost.Id))
            {
                blogPost.Slug = await GenerateSlugAsync(blogPost.Title, blogPost.Id);
            }

            // Set published date if being published for the first time
            if (blogPost.IsPublished && !existingBlogPost.IsPublished && !blogPost.PublishedDate.HasValue)
            {
                blogPost.PublishedDate = DateTime.UtcNow;
            }

            // Clean and format tags
            if (!string.IsNullOrEmpty(blogPost.Tags))
            {
                blogPost.Tags = CleanTags(blogPost.Tags);
            }

            // Update properties
            existingBlogPost.Title = blogPost.Title;
            existingBlogPost.Content = blogPost.Content;
            existingBlogPost.Author = blogPost.Author;
            existingBlogPost.ImageUrl = blogPost.ImageUrl;
            existingBlogPost.Summary = blogPost.Summary;
            existingBlogPost.Slug = blogPost.Slug;
            existingBlogPost.MetaDescription = blogPost.MetaDescription;
            existingBlogPost.MetaKeywords = blogPost.MetaKeywords;
            existingBlogPost.IsPublished = blogPost.IsPublished;
            existingBlogPost.IsFeatured = blogPost.IsFeatured;
            existingBlogPost.PublishedDate = blogPost.PublishedDate;
            existingBlogPost.Category = blogPost.Category;
            existingBlogPost.Tags = blogPost.Tags;
            existingBlogPost.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Delete old image if new one was uploaded
            if (imageFile != null && !string.IsNullOrEmpty(oldImagePath))
            {
                _imageUploadService.DeleteImage(oldImagePath);
            }

            _logger.LogInformation("Updated blog post with ID: {Id}", blogPost.Id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating blog post with ID: {Id}", blogPost.Id);

            // Clean up new image if upload failed
            if (imageFile != null && !string.IsNullOrEmpty(blogPost.ImageUrl))
            {
                _imageUploadService.DeleteImage(blogPost.ImageUrl);
            }

            return false;
        }
    }

    public async Task<bool> DeleteBlogPostAsync(int id)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                _logger.LogWarning("Blog post not found for soft delete: {Id}", id);
                return false;
            }

            blogPost.IsActive = false;
            blogPost.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted blog post with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting blog post with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> HardDeleteBlogPostAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var blogPost = await _context.BlogPosts.Include(bp => bp.Comments).FirstOrDefaultAsync(bp => bp.Id == id);
            if (blogPost == null)
            {
                _logger.LogWarning("Blog post not found for hard delete: {Id}", id);
                return false;
            }

            var imagePath = blogPost.ImageUrl;

            // Remove associated comments first
            _context.Comments.RemoveRange(blogPost.Comments);

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Delete feature image file
            if (!string.IsNullOrEmpty(imagePath))
            {
                _imageUploadService.DeleteImage(imagePath);
            }

            _logger.LogInformation("Hard deleted blog post with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error hard deleting blog post with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> TogglePublishedStatusAsync(int id)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                _logger.LogWarning("Blog post not found for status toggle: {Id}", id);
                return false;
            }

            blogPost.IsPublished = !blogPost.IsPublished;
            blogPost.UpdatedDate = DateTime.UtcNow;

            // Set published date if being published for the first time
            if (blogPost.IsPublished && !blogPost.PublishedDate.HasValue)
            {
                blogPost.PublishedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled blog post published status for ID: {Id} to {Status}", id, blogPost.IsPublished);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling blog post published status for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ToggleFeaturedStatusAsync(int id)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                _logger.LogWarning("Blog post not found for featured toggle: {Id}", id);
                return false;
            }

            blogPost.IsFeatured = !blogPost.IsFeatured;
            blogPost.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled blog post featured status for ID: {Id} to {Status}", id, blogPost.IsFeatured);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling blog post featured status for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> IncrementViewCountAsync(int id)
    {
        try
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return false;
            }

            blogPost.ViewCount++;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for blog post ID: {Id}", id);
            return false;
        }
    }

    public async Task<string> GenerateSlugAsync(string title, int? existingId = null)
    {
        try
        {
            var baseSlug = CreateSlugFromTitle(title);
            var slug = baseSlug;
            var counter = 1;

            while (!await IsSlugUniqueAsync(slug, existingId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating slug for title: {Title}", title);
            return CreateSlugFromTitle(title);
        }
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? existingId = null)
    {
        try
        {
            var query = _context.BlogPosts.Where(bp => bp.Slug == slug);
            
            if (existingId.HasValue)
            {
                query = query.Where(bp => bp.Id != existingId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking slug uniqueness: {Slug}", slug);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        try
        {
            return await _context.BlogPosts
                .Where(bp => !string.IsNullOrEmpty(bp.Category))
                .Select(bp => bp.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetTagsAsync()
    {
        try
        {
            var allTags = await _context.BlogPosts
                .Where(bp => !string.IsNullOrEmpty(bp.Tags))
                .Select(bp => bp.Tags!)
                .ToListAsync();

            return allTags
                .SelectMany(tags => tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Distinct()
                .OrderBy(tag => tag)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetAuthorsAsync()
    {
        try
        {
            return await _context.BlogPosts
                .Select(bp => bp.Author)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authors");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<BlogPostStatistics> GetBlogPostStatisticsAsync()
    {
        try
        {
            var totalPosts = await _context.BlogPosts.CountAsync();
            var publishedPosts = await _context.BlogPosts.CountAsync(bp => bp.IsPublished);
            var featuredPosts = await _context.BlogPosts.CountAsync(bp => bp.IsFeatured);
            var totalViews = await _context.BlogPosts.SumAsync(bp => bp.ViewCount);
            var totalComments = await _context.BlogPosts.SumAsync(bp => bp.Comments.Count);
            
            var lastPublished = await _context.BlogPosts
                .Where(bp => bp.IsPublished && bp.PublishedDate.HasValue)
                .MaxAsync(bp => (DateTime?)bp.PublishedDate);

            var mostViewedPost = await _context.BlogPosts
                .OrderByDescending(bp => bp.ViewCount)
                .FirstOrDefaultAsync();

            var latestPost = await _context.BlogPosts
                .OrderByDescending(bp => bp.CreatedDate)
                .FirstOrDefaultAsync();

            var categoriesCount = await _context.BlogPosts
                .Where(bp => !string.IsNullOrEmpty(bp.Category))
                .Select(bp => bp.Category)
                .Distinct()
                .CountAsync();

            var tagsCount = (await GetTagsAsync()).Count();

            return new BlogPostStatistics
            {
                TotalPosts = totalPosts,
                PublishedPosts = publishedPosts,
                DraftPosts = totalPosts - publishedPosts,
                FeaturedPosts = featuredPosts,
                TotalViews = totalViews,
                TotalComments = totalComments,
                CategoriesCount = categoriesCount,
                TagsCount = tagsCount,
                LastPublished = lastPublished,
                MostViewedPost = mostViewedPost,
                LatestPost = latestPost
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blog post statistics");
            return new BlogPostStatistics();
        }
    }

    public async Task<IEnumerable<BlogPost>> GetRelatedBlogPostsAsync(int blogPostId, int count = 5)
    {
        try
        {
            var currentPost = await _context.BlogPosts.FindAsync(blogPostId);
            if (currentPost == null)
            {
                return Enumerable.Empty<BlogPost>();
            }

            var query = _context.BlogPosts
                .Where(bp => bp.Id != blogPostId && bp.IsPublished);

            // First try to find posts in the same category
            if (!string.IsNullOrEmpty(currentPost.Category))
            {
                var categoryPosts = await query
                    .Where(bp => bp.Category == currentPost.Category)
                    .OrderByDescending(bp => bp.PublishedDate)
                    .Take(count)
                    .ToListAsync();

                if (categoryPosts.Count >= count)
                {
                    return categoryPosts;
                }

                // If not enough category posts, get more based on tags
                var remainingCount = count - categoryPosts.Count;
                var additionalPosts = await GetPostsByTags(currentPost.Tags, blogPostId, remainingCount, categoryPosts.Select(p => p.Id));
                
                return categoryPosts.Concat(additionalPosts).Take(count);
            }

            // If no category, find by tags
            return await GetPostsByTags(currentPost.Tags, blogPostId, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related blog posts for ID: {Id}", blogPostId);
            return Enumerable.Empty<BlogPost>();
        }
    }

    public async Task<bool> BlogPostExistsAsync(int id)
    {
        try
        {
            return await _context.BlogPosts.AnyAsync(bp => bp.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if blog post exists: {Id}", id);
            return false;
        }
    }

    #region Private Helper Methods

    private static string CreateSlugFromTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return "untitled";

        // Convert to lowercase and replace spaces with hyphens
        var slug = title.ToLowerInvariant().Trim();
        
        // Remove special characters and keep only letters, numbers, and hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Replace multiple spaces/hyphens with single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");
        
        // Remove leading/trailing hyphens
        slug = slug.Trim('-');
        
        // Ensure slug isn't empty
        if (string.IsNullOrEmpty(slug))
            slug = "untitled";
            
        // Limit length
        if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        return slug;
    }

    private static string CleanTags(string tags)
    {
        if (string.IsNullOrEmpty(tags))
            return string.Empty;

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(tag => tag.Trim())
                         .Where(tag => !string.IsNullOrEmpty(tag))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();

        return string.Join(", ", tagList);
    }

    private async Task<IEnumerable<BlogPost>> GetPostsByTags(string? tags, int excludeId, int count, IEnumerable<int>? excludeIds = null)
    {
        if (string.IsNullOrEmpty(tags))
        {
            // If no tags, return latest posts
            var query = _context.BlogPosts
                .Where(bp => bp.Id != excludeId && bp.IsPublished);

            if (excludeIds != null)
            {
                query = query.Where(bp => !excludeIds.Contains(bp.Id));
            }

            return await query
                .OrderByDescending(bp => bp.PublishedDate)
                .Take(count)
                .ToListAsync();
        }

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(t => t.Trim().ToLower())
                         .ToList();

        var postsQuery = _context.BlogPosts
            .Where(bp => bp.Id != excludeId && bp.IsPublished && bp.Tags != null);

        if (excludeIds != null)
        {
            postsQuery = postsQuery.Where(bp => !excludeIds.Contains(bp.Id));
        }

        var posts = await postsQuery.ToListAsync();

        // Score posts by tag matches
        var scoredPosts = posts
            .Select(post => new
            {
                Post = post,
                Score = CalculateTagScore(post.Tags!, tagList)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Post.PublishedDate)
            .Take(count)
            .Select(x => x.Post);

        return scoredPosts;
    }

    private static int CalculateTagScore(string postTags, List<string> targetTags)
    {
        var postTagList = postTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim().ToLower())
                                 .ToList();

        return targetTags.Count(tag => postTagList.Contains(tag));
    }

    #endregion
}

