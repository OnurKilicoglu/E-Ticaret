using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for ContactMessage entity
/// </summary>
public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        // Table name
        builder.ToTable("ContactMessages");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsReplied)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AdminReply)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.Email)
            .HasDatabaseName("IX_ContactMessages_Email");

        builder.HasIndex(x => x.IsRead)
            .HasDatabaseName("IX_ContactMessages_IsRead");

        builder.HasIndex(x => x.IsReplied)
            .HasDatabaseName("IX_ContactMessages_IsReplied");

        builder.HasIndex(x => x.CreatedDate)
            .HasDatabaseName("IX_ContactMessages_CreatedDate");

        builder.HasIndex(x => new { x.IsRead, x.CreatedDate })
            .HasDatabaseName("IX_ContactMessages_IsRead_CreatedDate");
    }
}
