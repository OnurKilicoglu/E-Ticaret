using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Data.Configurations;

/// <summary>
/// Entity configuration for Payment entity
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Table name
        builder.ToTable("Payments");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.PaymentMethod)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.PaymentStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PaymentDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.TransactionId)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentGateway)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentDetails)
            .HasMaxLength(500);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => x.OrderId)
            .IsUnique()
            .HasDatabaseName("IX_Payments_OrderId");

        builder.HasIndex(x => x.TransactionId)
            .IsUnique()
            .HasDatabaseName("IX_Payments_TransactionId")
            .HasFilter("[TransactionId] IS NOT NULL");

        builder.HasIndex(x => x.PaymentStatus)
            .HasDatabaseName("IX_Payments_PaymentStatus");

        builder.HasIndex(x => x.PaymentMethod)
            .HasDatabaseName("IX_Payments_PaymentMethod");

        // Relationships
        builder.HasOne(x => x.Order)
            .WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
