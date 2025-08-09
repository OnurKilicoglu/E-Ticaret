using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for AppUser entity
/// </summary>
public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        // Table name
        builder.ToTable("AppUsers");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FirstName)
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .HasMaxLength(50);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("IX_AppUsers_Email");

        builder.HasIndex(x => x.UserName)
            .IsUnique()
            .HasDatabaseName("IX_AppUsers_UserName");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_AppUsers_IsActive");

        // Relationships
        builder.HasMany(x => x.Orders)
            .WithOne(x => x.AppUser)
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Carts)
            .WithOne(x => x.AppUser)
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ShippingAddresses)
            .WithOne(x => x.AppUser)
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
