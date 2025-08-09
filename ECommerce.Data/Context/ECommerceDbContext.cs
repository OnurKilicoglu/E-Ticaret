using ECommerce.Core.Entities;
using ECommerce.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data.Context;

/// <summary>
/// Entity Framework DbContext for the E-Commerce application
/// </summary>
public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    #region DbSets

    // User & Identity
    public DbSet<AppUser> AppUsers { get; set; }

    // Product Catalog
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    // Shopping & Orders
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ShippingAddress> ShippingAddresses { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Content Management
    public DbSet<Slider> Sliders { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<FAQCategory> FAQCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new AppUserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        modelBuilder.ApplyConfiguration(new CartConfiguration());
        modelBuilder.ApplyConfiguration(new CartItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new ShippingAddressConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new SliderConfiguration());
        modelBuilder.ApplyConfiguration(new ContactMessageConfiguration());
        modelBuilder.ApplyConfiguration(new FAQConfiguration());
        modelBuilder.ApplyConfiguration(new FAQCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());

        // Global query filters for soft delete
        ConfigureGlobalQueryFilters(modelBuilder);
    }

    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply global query filters for entities with IsActive property (soft delete)
        modelBuilder.Entity<AppUser>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Slider>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<FAQ>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<FAQCategory>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<BlogPost>().HasQueryFilter(e => e.IsPublished);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                    break;
            }
        }
    }
}
