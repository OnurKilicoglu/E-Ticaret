using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for FAQ operations
/// </summary>
public class FAQService : IFAQService
{
    private readonly ECommerceDbContext _context;
    private readonly ILogger<FAQService> _logger;

    public FAQService(ECommerceDbContext context, ILogger<FAQService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region FAQ Operations

    public async Task<(IEnumerable<FAQ> FAQs, int TotalCount)> GetFAQsAsync(
        string? searchTerm = null, int? categoryId = null, bool? isActive = null,
        string? tags = null, string? author = null, string sortBy = "displayOrder",
        string sortOrder = "asc", int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.FAQs.Include(f => f.Category).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f => f.Question.Contains(searchTerm) || f.Answer.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.IgnoreQueryFilters().Where(f => f.IsActive == isActive.Value);
            }

            if (!string.IsNullOrEmpty(tags))
            {
                query = query.Where(f => f.Tags != null && f.Tags.Contains(tags));
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(f => f.Author != null && f.Author.Contains(author));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "question" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(f => f.Question)
                    : query.OrderBy(f => f.Question),
                "viewcount" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(f => f.ViewCount)
                    : query.OrderBy(f => f.ViewCount),
                "createddate" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(f => f.CreatedDate)
                    : query.OrderBy(f => f.CreatedDate),
                "category" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(f => f.Category != null ? f.Category.Name : "")
                    : query.OrderBy(f => f.Category != null ? f.Category.Name : ""),
                "helpfulness" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(f => f.HelpfulCount - f.NotHelpfulCount)
                    : query.OrderBy(f => f.HelpfulCount - f.NotHelpfulCount),
                _ => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(f => f.DisplayOrder)
                    : query.OrderBy(f => f.DisplayOrder)
            };

            // Apply pagination
            var faqs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (faqs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQs with filters");
            return (Enumerable.Empty<FAQ>(), 0);
        }
    }

    public async Task<FAQ?> GetFAQByIdAsync(int id)
    {
        try
        {
            return await _context.FAQs
                .Include(f => f.Category)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQ by ID: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<FAQ>> GetActiveFAQsAsync(int? categoryId = null, int? limit = null)
    {
        try
        {
            IQueryable<FAQ> query = _context.FAQs
                .Include(f => f.Category)
                .Where(f => f.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            query = query.OrderBy(f => f.DisplayOrder).ThenBy(f => f.Question);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active FAQs");
            return Enumerable.Empty<FAQ>();
        }
    }

    public async Task<FAQ?> CreateFAQAsync(FAQ faq)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Set creation date
            faq.CreatedDate = DateTime.UtcNow;

            // Auto-set display order if not provided
            if (faq.DisplayOrder == 0)
            {
                faq.DisplayOrder = await GetNextFAQDisplayOrderAsync(faq.CategoryId);
            }

            _context.FAQs.Add(faq);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Created FAQ with ID: {Id}", faq.Id);
            return faq;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating FAQ");
            return null;
        }
    }

    public async Task<bool> UpdateFAQAsync(FAQ faq)
    {
        try
        {
            var existingFAQ = await _context.FAQs.FindAsync(faq.Id);
            if (existingFAQ == null)
            {
                _logger.LogWarning("FAQ not found for update: {Id}", faq.Id);
                return false;
            }

            // Update properties
            existingFAQ.Question = faq.Question;
            existingFAQ.Answer = faq.Answer;
            existingFAQ.CategoryId = faq.CategoryId;
            existingFAQ.DisplayOrder = faq.DisplayOrder;
            existingFAQ.IsActive = faq.IsActive;
            existingFAQ.Tags = faq.Tags;
            existingFAQ.Author = faq.Author;
            existingFAQ.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated FAQ with ID: {Id}", faq.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ with ID: {Id}", faq.Id);
            return false;
        }
    }

    public async Task<bool> DeleteFAQAsync(int id)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                _logger.LogWarning("FAQ not found for soft delete: {Id}", id);
                return false;
            }

            faq.IsActive = false;
            faq.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted FAQ with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting FAQ with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> HardDeleteFAQAsync(int id)
    {
        try
        {
            var faq = await _context.FAQs.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id);
            if (faq == null)
            {
                _logger.LogWarning("FAQ not found for hard delete: {Id}", id);
                return false;
            }

            _context.FAQs.Remove(faq);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Hard deleted FAQ with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hard deleting FAQ with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ToggleFAQStatusAsync(int id)
    {
        try
        {
            var faq = await _context.FAQs.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id);
            if (faq == null)
            {
                _logger.LogWarning("FAQ not found for status toggle: {Id}", id);
                return false;
            }

            faq.IsActive = !faq.IsActive;
            faq.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled FAQ status for ID: {Id} to {Status}", id, faq.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling FAQ status for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateFAQOrderAsync(int id, int newOrder)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                _logger.LogWarning("FAQ not found for order update: {Id}", id);
                return false;
            }

            faq.DisplayOrder = newOrder;
            faq.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated FAQ order for ID: {Id} to {Order}", id, newOrder);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ order for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> BulkUpdateFAQOrdersAsync(Dictionary<int, int> orderUpdates)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var update in orderUpdates)
            {
                var faq = await _context.FAQs.FindAsync(update.Key);
                if (faq != null)
                {
                    faq.DisplayOrder = update.Value;
                    faq.UpdatedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Bulk updated FAQ orders for {Count} items", orderUpdates.Count);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error bulk updating FAQ orders");
            return false;
        }
    }

    public async Task<bool> IncrementViewCountAsync(int id)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return false;
            }

            faq.ViewCount++;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for FAQ ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> MarkFAQHelpfulnessAsync(int id, bool isHelpful)
    {
        try
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null)
            {
                return false;
            }

            if (isHelpful)
            {
                faq.HelpfulCount++;
            }
            else
            {
                faq.NotHelpfulCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking FAQ helpfulness for ID: {Id}", id);
            return false;
        }
    }

    public async Task<IEnumerable<FAQ>> SearchFAQsAsync(string keyword, int? categoryId = null, int limit = 10)
    {
        try
        {
            var query = _context.FAQs
                .Include(f => f.Category)
                .Where(f => f.IsActive && (f.Question.Contains(keyword) || f.Answer.Contains(keyword)));

            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            return await query
                .OrderBy(f => f.DisplayOrder)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching FAQs with keyword: {Keyword}", keyword);
            return Enumerable.Empty<FAQ>();
        }
    }

    #endregion

    #region FAQ Category Operations

    public async Task<IEnumerable<FAQCategory>> GetFAQCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            var query = _context.FAQCategories.AsQueryable();

            if (includeInactive)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQ categories");
            return Enumerable.Empty<FAQCategory>();
        }
    }

    public async Task<FAQCategory?> GetFAQCategoryByIdAsync(int id)
    {
        try
        {
            return await _context.FAQCategories
                .Include(c => c.FAQs)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQ category by ID: {Id}", id);
            return null;
        }
    }

    public async Task<FAQCategory?> CreateFAQCategoryAsync(FAQCategory category)
    {
        try
        {
            // Check name uniqueness
            if (!await IsCategoryNameUniqueAsync(category.Name))
            {
                _logger.LogWarning("FAQ category name already exists: {Name}", category.Name);
                return null;
            }

            // Set creation date
            category.CreatedDate = DateTime.UtcNow;

            // Auto-set display order if not provided
            if (category.DisplayOrder == 0)
            {
                category.DisplayOrder = await GetNextCategoryDisplayOrderAsync();
            }

            _context.FAQCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created FAQ category with ID: {Id}", category.Id);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating FAQ category");
            return null;
        }
    }

    public async Task<bool> UpdateFAQCategoryAsync(FAQCategory category)
    {
        try
        {
            var existingCategory = await _context.FAQCategories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                _logger.LogWarning("FAQ category not found for update: {Id}", category.Id);
                return false;
            }

            // Check name uniqueness
            if (!await IsCategoryNameUniqueAsync(category.Name, category.Id))
            {
                _logger.LogWarning("FAQ category name already exists: {Name}", category.Name);
                return false;
            }

            // Update properties
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Icon = category.Icon;
            existingCategory.DisplayOrder = category.DisplayOrder;
            existingCategory.IsActive = category.IsActive;
            existingCategory.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated FAQ category with ID: {Id}", category.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ category with ID: {Id}", category.Id);
            return false;
        }
    }

    public async Task<bool> DeleteFAQCategoryAsync(int id)
    {
        try
        {
            var category = await _context.FAQCategories
                .Include(c => c.FAQs)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                _logger.LogWarning("FAQ category not found for delete: {Id}", id);
                return false;
            }

            // Set FAQs to no category
            foreach (var faq in category.FAQs)
            {
                faq.CategoryId = null;
            }

            category.IsActive = false;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted FAQ category with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FAQ category with ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ToggleFAQCategoryStatusAsync(int id)
    {
        try
        {
            var category = await _context.FAQCategories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                _logger.LogWarning("FAQ category not found for status toggle: {Id}", id);
                return false;
            }

            category.IsActive = !category.IsActive;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled FAQ category status for ID: {Id} to {Status}", id, category.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling FAQ category status for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateFAQCategoryOrderAsync(int id, int newOrder)
    {
        try
        {
            var category = await _context.FAQCategories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("FAQ category not found for order update: {Id}", id);
                return false;
            }

            category.DisplayOrder = newOrder;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated FAQ category order for ID: {Id} to {Order}", id, newOrder);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FAQ category order for ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null)
    {
        try
        {
            var query = _context.FAQCategories.IgnoreQueryFilters().Where(c => c.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking category name uniqueness: {Name}", name);
            return false;
        }
    }

    #endregion

    #region Statistics and Analytics

    public async Task<FAQStatistics> GetFAQStatisticsAsync()
    {
        try
        {
            var totalFAQs = await _context.FAQs.IgnoreQueryFilters().CountAsync();
            var activeFAQs = await _context.FAQs.CountAsync();
            var totalCategories = await _context.FAQCategories.IgnoreQueryFilters().CountAsync();
            var activeCategories = await _context.FAQCategories.CountAsync();
            var totalViews = await _context.FAQs.SumAsync(f => f.ViewCount);
            var totalHelpfulVotes = await _context.FAQs.SumAsync(f => f.HelpfulCount);
            var totalNotHelpfulVotes = await _context.FAQs.SumAsync(f => f.NotHelpfulCount);

            var mostViewedFAQ = await _context.FAQs
                .OrderByDescending(f => f.ViewCount)
                .FirstOrDefaultAsync();

            var mostHelpfulFAQ = await _context.FAQs
                .OrderByDescending(f => f.HelpfulCount - f.NotHelpfulCount)
                .FirstOrDefaultAsync();

            var mostPopularCategory = await _context.FAQCategories
                .Include(c => c.FAQs)
                .OrderByDescending(c => c.FAQs.Count)
                .FirstOrDefaultAsync();

            var lastUpdated = await _context.FAQs
                .OrderByDescending(f => f.UpdatedDate ?? f.CreatedDate)
                .Select(f => f.UpdatedDate ?? f.CreatedDate)
                .FirstOrDefaultAsync();

            return new FAQStatistics
            {
                TotalFAQs = totalFAQs,
                ActiveFAQs = activeFAQs,
                InactiveFAQs = totalFAQs - activeFAQs,
                TotalCategories = totalCategories,
                ActiveCategories = activeCategories,
                TotalViews = totalViews,
                TotalHelpfulVotes = totalHelpfulVotes,
                TotalNotHelpfulVotes = totalNotHelpfulVotes,
                MostViewedFAQ = mostViewedFAQ,
                MostHelpfulFAQ = mostHelpfulFAQ,
                MostPopularCategory = mostPopularCategory,
                LastUpdated = lastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FAQ statistics");
            return new FAQStatistics();
        }
    }

    public async Task<IEnumerable<FAQ>> GetMostViewedFAQsAsync(int count = 5)
    {
        try
        {
            return await _context.FAQs
                .Include(f => f.Category)
                .OrderByDescending(f => f.ViewCount)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most viewed FAQs");
            return Enumerable.Empty<FAQ>();
        }
    }

    public async Task<IEnumerable<FAQ>> GetMostHelpfulFAQsAsync(int count = 5)
    {
        try
        {
            return await _context.FAQs
                .Include(f => f.Category)
                .OrderByDescending(f => f.HelpfulCount - f.NotHelpfulCount)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most helpful FAQs");
            return Enumerable.Empty<FAQ>();
        }
    }

    public async Task<IEnumerable<FAQCategoryWithCount>> GetCategoriesWithCountsAsync()
    {
        try
        {
            return await _context.FAQCategories
                .Include(c => c.FAQs)
                .Select(c => new FAQCategoryWithCount
                {
                    Category = c,
                    FAQCount = c.FAQs.Count,
                    ActiveFAQCount = c.FAQs.Count(f => f.IsActive)
                })
                .OrderBy(c => c.Category.DisplayOrder)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories with counts");
            return Enumerable.Empty<FAQCategoryWithCount>();
        }
    }

    #endregion

    #region Utility Methods

    public async Task<bool> FAQExistsAsync(int id)
    {
        try
        {
            return await _context.FAQs.IgnoreQueryFilters().AnyAsync(f => f.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if FAQ exists: {Id}", id);
            return false;
        }
    }

    public async Task<bool> FAQCategoryExistsAsync(int id)
    {
        try
        {
            return await _context.FAQCategories.IgnoreQueryFilters().AnyAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if FAQ category exists: {Id}", id);
            return false;
        }
    }

    public async Task<int> GetNextFAQDisplayOrderAsync(int? categoryId = null)
    {
        try
        {
            var query = _context.FAQs.IgnoreQueryFilters().AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            var maxOrder = await query.MaxAsync(f => (int?)f.DisplayOrder) ?? 0;
            return maxOrder + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next FAQ display order");
            return 1;
        }
    }

    public async Task<int> GetNextCategoryDisplayOrderAsync()
    {
        try
        {
            var maxOrder = await _context.FAQCategories
                .IgnoreQueryFilters()
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
            return maxOrder + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next category display order");
            return 1;
        }
    }

    #endregion
}
