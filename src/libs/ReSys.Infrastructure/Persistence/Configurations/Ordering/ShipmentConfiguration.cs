using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable(DatabaseTables.Shipments, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
            .HasMaxLength(ShipmentConstraints.NumberMaxLength)
            .IsRequired();

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(ShipmentConstraints.TrackingNumberMaxLength);

        builder.HasMany(x => x.InventoryUnits)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.StockLocationId);
    }
}
