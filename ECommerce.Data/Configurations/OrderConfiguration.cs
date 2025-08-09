using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for Order entity
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Table name
        builder.ToTable("Orders");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.OrderDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.SubTotal)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.TaxAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.ShippingCost)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.OrderStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(OrderStatus.Pending);

        builder.Property(x => x.OrderNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.AppUserId)
            .HasDatabaseName("IX_Orders_AppUserId");

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber")
            .HasFilter("[OrderNumber] IS NOT NULL");

        builder.HasIndex(x => x.OrderStatus)
            .HasDatabaseName("IX_Orders_OrderStatus");

        builder.HasIndex(x => x.OrderDate)
            .HasDatabaseName("IX_Orders_OrderDate");

        builder.HasIndex(x => x.ShippingAddressId)
            .HasDatabaseName("IX_Orders_ShippingAddressId");

        builder.HasIndex(x => x.TrackingNumber)
            .HasDatabaseName("IX_Orders_TrackingNumber")
            .HasFilter("[TrackingNumber] IS NOT NULL");

        // Relationships
        builder.HasOne(x => x.AppUser)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ShippingAddress)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Payment)
            .WithOne(x => x.Order)
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
