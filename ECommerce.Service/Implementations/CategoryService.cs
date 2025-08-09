using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for Category entity operations
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ECommerceDbContext _context;

    public CategoryService(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Category> Categories, int TotalCount)> GetCategoriesAsync(
        string? searchTerm = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Categories.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm) || 
                                   (c.Description != null && c.Description.Contains(searchTerm)));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (categories, totalCount);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.CreatedDate = DateTime.UtcNow;
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        category.UpdatedDate = DateTime.UtcNow;
        
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await GetCategoryByIdAsync(id);
        if (category == null)
            return false;

        // Check if category has products
        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            // Soft delete - just set IsActive to false
            category.IsActive = false;
            category.UpdatedDate = DateTime.UtcNow;
            _context.Categories.Update(category);
        }
        else
        {
            // Hard delete if no products
            _context.Categories.Remove(category);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> GetCategoryCountAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .CountAsync();
    }
}
