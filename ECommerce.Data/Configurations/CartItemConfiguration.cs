using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for CartItem entity
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        // Table name
        builder.ToTable("CartItems");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.CartId)
            .HasDatabaseName("IX_CartItems_CartId");

        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName("IX_CartItems_ProductId");

        builder.HasIndex(x => new { x.CartId, x.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_CartId_ProductId");

        // Relationships
        builder.HasOne(x => x.Cart)
            .WithMany(x => x.CartItems)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.CartItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
