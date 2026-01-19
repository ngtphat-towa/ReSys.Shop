using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Ordering;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(DatabaseTables.Orders, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
            .HasMaxLength(OrderConstraints.NumberMaxLength)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(OrderConstraints.CurrencyCodeLength)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(OrderConstraints.EmailMaxLength);

        // State machine
        builder.Property(x => x.State);

        // Financials (long)
        builder.Property(x => x.ItemTotalCents);
        builder.Property(x => x.ShipmentTotalCents);
        builder.Property(x => x.AdjustmentTotalCents);
        builder.Property(x => x.TotalCents);

        // Metadata JSONB
        builder.Property(x => x.PublicMetadata).HasColumnType("jsonb");
        builder.Property(x => x.PrivateMetadata).HasColumnType("jsonb");

        builder.HasMany(x => x.LineItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderAdjustments)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Shipments)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Histories)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Email);
    }
}
