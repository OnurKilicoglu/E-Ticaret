using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for Cart entity
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        // Table name
        builder.ToTable("Carts");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.SessionId)
            .HasMaxLength(255);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.AppUserId)
            .HasDatabaseName("IX_Carts_AppUserId");

        builder.HasIndex(x => x.SessionId)
            .HasDatabaseName("IX_Carts_SessionId")
            .HasFilter("[SessionId] IS NOT NULL");

        // Relationships
        builder.HasOne(x => x.AppUser)
            .WithMany(x => x.Carts)
            .HasForeignKey(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CartItems)
            .WithOne(x => x.Cart)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
