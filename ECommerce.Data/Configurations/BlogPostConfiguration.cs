using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for BlogPost entity
/// </summary>
public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        // Table name
        builder.ToTable("BlogPosts");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(255);

        builder.Property(x => x.Summary)
            .HasMaxLength(500);

        builder.Property(x => x.Slug)
            .HasMaxLength(200);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(160);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(200);

        builder.Property(x => x.IsPublished)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Category)
            .HasMaxLength(100);

        builder.Property(x => x.Tags)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("IX_BlogPosts_Slug")
            .HasFilter("[Slug] IS NOT NULL");

        builder.HasIndex(x => x.IsPublished)
            .HasDatabaseName("IX_BlogPosts_IsPublished");

        builder.HasIndex(x => x.IsFeatured)
            .HasDatabaseName("IX_BlogPosts_IsFeatured");

        builder.HasIndex(x => x.PublishedDate)
            .HasDatabaseName("IX_BlogPosts_PublishedDate");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("IX_BlogPosts_Category")
            .HasFilter("[Category] IS NOT NULL");

        builder.HasIndex(x => new { x.IsPublished, x.PublishedDate })
            .HasDatabaseName("IX_BlogPosts_IsPublished_PublishedDate");

        builder.HasIndex(x => new { x.IsPublished, x.IsFeatured })
            .HasDatabaseName("IX_BlogPosts_IsPublished_IsFeatured");

        // Relationships
        builder.HasMany(x => x.Comments)
            .WithOne(x => x.BlogPost)
            .HasForeignKey(x => x.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
