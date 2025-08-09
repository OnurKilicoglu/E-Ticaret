using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for Comment entity
/// </summary>
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Table name
        builder.ToTable("Comments");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Website)
            .HasMaxLength(255);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsSpam)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.BlogPostId)
            .HasDatabaseName("IX_Comments_BlogPostId");

        builder.HasIndex(x => x.ParentCommentId)
            .HasDatabaseName("IX_Comments_ParentCommentId")
            .HasFilter("[ParentCommentId] IS NOT NULL");

        builder.HasIndex(x => x.IsApproved)
            .HasDatabaseName("IX_Comments_IsApproved");

        builder.HasIndex(x => x.IsSpam)
            .HasDatabaseName("IX_Comments_IsSpam");

        builder.HasIndex(x => x.Email)
            .HasDatabaseName("IX_Comments_Email");

        builder.HasIndex(x => new { x.BlogPostId, x.IsApproved })
            .HasDatabaseName("IX_Comments_BlogPostId_IsApproved");

        builder.HasIndex(x => new { x.BlogPostId, x.ParentCommentId })
            .HasDatabaseName("IX_Comments_BlogPostId_ParentCommentId")
            .HasFilter("[ParentCommentId] IS NOT NULL");

        // Relationships
        builder.HasOne(x => x.BlogPost)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for replies
        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
