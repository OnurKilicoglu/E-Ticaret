using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for Slider entity
/// </summary>
public class SliderConfiguration : IEntityTypeConfiguration<Slider>
{
    public void Configure(EntityTypeBuilder<Slider> builder)
    {
        // Table name
        builder.ToTable("Sliders");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Link)
            .HasMaxLength(255);

        builder.Property(x => x.ButtonText)
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Sliders_IsActive");

        builder.HasIndex(x => x.DisplayOrder)
            .HasDatabaseName("IX_Sliders_DisplayOrder");

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("IX_Sliders_IsActive_DisplayOrder");
    }
}
