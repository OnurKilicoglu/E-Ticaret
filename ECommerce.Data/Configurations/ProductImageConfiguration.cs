using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for ProductImage entity
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        // Table name
        builder.ToTable("ProductImages");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.AltText)
            .HasMaxLength(100);

        builder.Property(x => x.IsMain)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName("IX_ProductImages_ProductId");

        builder.HasIndex(x => new { x.ProductId, x.IsMain })
            .HasDatabaseName("IX_ProductImages_ProductId_IsMain");

        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder })
            .HasDatabaseName("IX_ProductImages_ProductId_DisplayOrder");

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductImages)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
