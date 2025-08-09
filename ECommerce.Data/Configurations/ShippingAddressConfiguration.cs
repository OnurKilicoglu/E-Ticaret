using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for ShippingAddress entity
/// </summary>
public class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddress>
{
    public void Configure(EntityTypeBuilder<ShippingAddress> builder)
    {
        // Table name
        builder.ToTable("ShippingAddresses");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(200);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ZipCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.AppUserId)
            .HasDatabaseName("IX_ShippingAddresses_AppUserId");

        builder.HasIndex(x => new { x.AppUserId, x.IsDefault })
            .HasDatabaseName("IX_ShippingAddresses_AppUserId_IsDefault");

        // Relationships
        builder.HasOne(x => x.AppUser)
            .WithMany(x => x.ShippingAddresses)
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Orders)
            .WithOne(x => x.ShippingAddress)
            .HasForeignKey(x => x.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
