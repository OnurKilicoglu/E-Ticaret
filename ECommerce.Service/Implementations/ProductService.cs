using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for Product entity operations
/// </summary>
public class ProductService : IProductService
{
    private readonly ECommerceDbContext _context;

    public ProductService(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        string? searchTerm = null, 
        int? categoryId = null, 
        string sortBy = "name", 
        string sortOrder = "asc", 
        int page = 1, 
        int pageSize = 10)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                   (p.Description != null && p.Description.Contains(searchTerm)) ||
                                   (p.SKU != null && p.SKU.Contains(searchTerm)) ||
                                   (p.Brand != null && p.Brand.Contains(searchTerm)));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "price" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "stock" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.StockQuantity)
                : query.OrderBy(p => p.StockQuantity),
            "created" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.CreatedDate)
                : query.OrderBy(p => p.CreatedDate),
            "category" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Category.Name)
                : query.OrderBy(p => p.Category.Name),
            _ => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.IsActive && p.Category.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.CreatedDate = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        product.UpdatedDate = DateTime.UtcNow;
        
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        
        return product;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await GetProductByIdAsync(id);
        if (product == null)
            return false;

        // Soft delete - set IsActive to false
        product.IsActive = false;
        product.UpdatedDate = DateTime.UtcNow;
        
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> HardDeleteProductAsync(int id)
    {
        var product = await GetProductByIdAsync(id);
        if (product == null)
            return false;

        // Check if product has orders (prevent deletion if has order history)
        var hasOrders = await _context.OrderItems
            .AnyAsync(oi => oi.ProductId == id);

        if (hasOrders)
        {
            // If has orders, just soft delete for data integrity
            return await DeleteProductAsync(id);
        }

        // Hard delete if no order history
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ProductNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Products.Where(p => p.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var query = _context.Products.Where(p => p.SKU != null && p.SKU.ToLower() == sku.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<int> GetProductCountAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .CountAsync();
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.IsActive && p.IsFeatured && p.Category.IsActive)
            .OrderByDescending(p => p.ViewCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        product.StockQuantity = quantity;
        product.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        product.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }
}
