using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity Framework configuration for FAQCategory entity
/// </summary>
public class FAQCategoryConfiguration : IEntityTypeConfiguration<FAQCategory>
{
    public void Configure(EntityTypeBuilder<FAQCategory> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedDate)
            .IsRequired(false);

        // Relationships
        builder.HasMany(x => x.FAQs)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.DisplayOrder);

        builder.HasIndex(x => x.IsActive);

        // Table Configuration
        builder.ToTable("FAQCategories");
    }
}

