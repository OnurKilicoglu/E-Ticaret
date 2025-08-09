using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for FAQ operations
/// </summary>
public interface IFAQService
{
    #region FAQ Operations
    
    /// <summary>
    /// Get all FAQs with filtering and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for question/answer</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="tags">Filter by tags</param>
    /// <param name="author">Filter by author</param>
    /// <param name="sortBy">Sort field (displayOrder, question, viewCount, createdDate)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing FAQs and total count</returns>
    Task<(IEnumerable<FAQ> FAQs, int TotalCount)> GetFAQsAsync(
        string? searchTerm = null,
        int? categoryId = null,
        bool? isActive = null,
        string? tags = null,
        string? author = null,
        string sortBy = "displayOrder",
        string sortOrder = "asc",
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get FAQ by ID
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <returns>FAQ entity or null if not found</returns>
    Task<FAQ?> GetFAQByIdAsync(int id);

    /// <summary>
    /// Get active FAQs for frontend display
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="limit">Maximum number of FAQs to return</param>
    /// <returns>List of active FAQs ordered by DisplayOrder</returns>
    Task<IEnumerable<FAQ>> GetActiveFAQsAsync(int? categoryId = null, int? limit = null);

    /// <summary>
    /// Create new FAQ
    /// </summary>
    /// <param name="faq">FAQ entity to create</param>
    /// <returns>Created FAQ or null if failed</returns>
    Task<FAQ?> CreateFAQAsync(FAQ faq);

    /// <summary>
    /// Update existing FAQ
    /// </summary>
    /// <param name="faq">FAQ entity to update</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateFAQAsync(FAQ faq);

    /// <summary>
    /// Delete FAQ (soft delete)
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteFAQAsync(int id);

    /// <summary>
    /// Hard delete FAQ
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <returns>True if successful</returns>
    Task<bool> HardDeleteFAQAsync(int id);

    /// <summary>
    /// Toggle FAQ active status
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <returns>True if successful</returns>
    Task<bool> ToggleFAQStatusAsync(int id);

    /// <summary>
    /// Update FAQ display order
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <param name="newOrder">New display order</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateFAQOrderAsync(int id, int newOrder);

    /// <summary>
    /// Bulk update FAQ display orders
    /// </summary>
    /// <param name="orderUpdates">Dictionary of FAQ ID to new order</param>
    /// <returns>True if successful</returns>
    Task<bool> BulkUpdateFAQOrdersAsync(Dictionary<int, int> orderUpdates);

    /// <summary>
    /// Increment FAQ view count
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <returns>True if successful</returns>
    Task<bool> IncrementViewCountAsync(int id);

    /// <summary>
    /// Mark FAQ as helpful or not helpful
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <param name="isHelpful">True for helpful, false for not helpful</param>
    /// <returns>True if successful</returns>
    Task<bool> MarkFAQHelpfulnessAsync(int id, bool isHelpful);

    /// <summary>
    /// Search FAQs by keyword
    /// </summary>
    /// <param name="keyword">Search keyword</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="limit">Maximum results</param>
    /// <returns>Matching FAQs</returns>
    Task<IEnumerable<FAQ>> SearchFAQsAsync(string keyword, int? categoryId = null, int limit = 10);

    #endregion

    #region FAQ Category Operations

    /// <summary>
    /// Get all FAQ categories
    /// </summary>
    /// <param name="includeInactive">Include inactive categories</param>
    /// <returns>List of FAQ categories</returns>
    Task<IEnumerable<FAQCategory>> GetFAQCategoriesAsync(bool includeInactive = false);

    /// <summary>
    /// Get FAQ category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>FAQ category or null if not found</returns>
    Task<FAQCategory?> GetFAQCategoryByIdAsync(int id);

    /// <summary>
    /// Create new FAQ category
    /// </summary>
    /// <param name="category">FAQ category to create</param>
    /// <returns>Created category or null if failed</returns>
    Task<FAQCategory?> CreateFAQCategoryAsync(FAQCategory category);

    /// <summary>
    /// Update existing FAQ category
    /// </summary>
    /// <param name="category">FAQ category to update</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateFAQCategoryAsync(FAQCategory category);

    /// <summary>
    /// Delete FAQ category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteFAQCategoryAsync(int id);

    /// <summary>
    /// Toggle FAQ category active status
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>True if successful</returns>
    Task<bool> ToggleFAQCategoryStatusAsync(int id);

    /// <summary>
    /// Update FAQ category display order
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="newOrder">New display order</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateFAQCategoryOrderAsync(int id, int newOrder);

    /// <summary>
    /// Check if category name is unique
    /// </summary>
    /// <param name="name">Category name</param>
    /// <param name="excludeId">Category ID to exclude from check</param>
    /// <returns>True if unique</returns>
    Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null);

    #endregion

    #region Statistics and Analytics

    /// <summary>
    /// Get FAQ statistics
    /// </summary>
    /// <returns>FAQ statistics</returns>
    Task<FAQStatistics> GetFAQStatisticsAsync();

    /// <summary>
    /// Get most viewed FAQs
    /// </summary>
    /// <param name="count">Number of FAQs to return</param>
    /// <returns>Most viewed FAQs</returns>
    Task<IEnumerable<FAQ>> GetMostViewedFAQsAsync(int count = 5);

    /// <summary>
    /// Get most helpful FAQs
    /// </summary>
    /// <param name="count">Number of FAQs to return</param>
    /// <returns>Most helpful FAQs</returns>
    Task<IEnumerable<FAQ>> GetMostHelpfulFAQsAsync(int count = 5);

    /// <summary>
    /// Get categories with FAQ counts
    /// </summary>
    /// <returns>Categories with FAQ counts</returns>
    Task<IEnumerable<FAQCategoryWithCount>> GetCategoriesWithCountsAsync();

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if FAQ exists
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <returns>True if exists</returns>
    Task<bool> FAQExistsAsync(int id);

    /// <summary>
    /// Check if FAQ category exists
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>True if exists</returns>
    Task<bool> FAQCategoryExistsAsync(int id);

    /// <summary>
    /// Get next display order for FAQ
    /// </summary>
    /// <param name="categoryId">Category ID (optional)</param>
    /// <returns>Next available display order</returns>
    Task<int> GetNextFAQDisplayOrderAsync(int? categoryId = null);

    /// <summary>
    /// Get next display order for FAQ category
    /// </summary>
    /// <returns>Next available display order</returns>
    Task<int> GetNextCategoryDisplayOrderAsync();

    #endregion
}

/// <summary>
/// FAQ statistics model
/// </summary>
public class FAQStatistics
{
    public int TotalFAQs { get; set; }
    public int ActiveFAQs { get; set; }
    public int InactiveFAQs { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int TotalViews { get; set; }
    public int TotalHelpfulVotes { get; set; }
    public int TotalNotHelpfulVotes { get; set; }
    public FAQ? MostViewedFAQ { get; set; }
    public FAQ? MostHelpfulFAQ { get; set; }
    public FAQCategory? MostPopularCategory { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// FAQ category with count model
/// </summary>
public class FAQCategoryWithCount
{
    public FAQCategory Category { get; set; } = null!;
    public int FAQCount { get; set; }
    public int ActiveFAQCount { get; set; }
}

