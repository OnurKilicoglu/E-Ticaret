using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for Product entity
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table name
        builder.ToTable("Products");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DiscountPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(255);

        builder.Property(x => x.SKU)
            .HasMaxLength(100);

        builder.Property(x => x.Brand)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasIndex(x => x.SKU)
            .IsUnique()
            .HasDatabaseName("IX_Products_SKU")
            .HasFilter("[SKU] IS NOT NULL");

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Products_IsActive");

        builder.HasIndex(x => x.IsFeatured)
            .HasDatabaseName("IX_Products_IsFeatured");

        builder.HasIndex(x => x.Price)
            .HasDatabaseName("IX_Products_Price");

        builder.HasIndex(x => new { x.IsActive, x.IsFeatured })
            .HasDatabaseName("IX_Products_IsActive_IsFeatured");

        // Relationships
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ProductImages)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.CartItems)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
