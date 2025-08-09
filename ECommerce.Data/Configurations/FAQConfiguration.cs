using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for FAQ entity
/// </summary>
public class FAQConfiguration : IEntityTypeConfiguration<FAQ>
{
    public void Configure(EntityTypeBuilder<FAQ> builder)
    {
        // Table name
        builder.ToTable("FAQs");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Answer)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Tags)
            .HasMaxLength(200);

        builder.Property(x => x.Author)
            .HasMaxLength(100);

        builder.Property(x => x.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedDate)
            .IsRequired(false);

        builder.Property(x => x.HelpfulCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.NotHelpfulCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Category)
            .WithMany(x => x.FAQs)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_FAQs_IsActive");

        builder.HasIndex(x => x.DisplayOrder)
            .HasDatabaseName("IX_FAQs_DisplayOrder");

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_FAQs_CategoryId");

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("IX_FAQs_IsActive_DisplayOrder");

        builder.HasIndex(x => new { x.IsActive, x.CategoryId })
            .HasDatabaseName("IX_FAQs_IsActive_CategoryId");

        builder.HasIndex(x => x.ViewCount)
            .HasDatabaseName("IX_FAQs_ViewCount");
    }
}
