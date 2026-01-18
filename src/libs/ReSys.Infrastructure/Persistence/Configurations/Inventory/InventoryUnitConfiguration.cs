using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Inventory;

public class InventoryUnitConfiguration : IEntityTypeConfiguration<InventoryUnit>
{
    public void Configure(EntityTypeBuilder<InventoryUnit> builder)
    {
        builder.ToTable(DatabaseTables.InventoryUnits, DatabaseSchemas.Catalog);
        builder.HasKey(x => x.Id);

        // Uses Postgres Native Enum
        builder.Property(x => x.State);

        builder.Property(x => x.SerialNumber)
            .HasMaxLength(InventoryUnitConstraints.SerialNumberMaxLength);

        builder.Property(x => x.LotNumber)
            .HasMaxLength(InventoryUnitConstraints.LotNumberMaxLength);

        builder.HasOne(x => x.StockItem)
            .WithMany(s => s.InventoryUnits)
            .HasForeignKey(x => x.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StockItemId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => x.VariantId);
        builder.HasIndex(x => x.LineItemId);
        builder.HasIndex(x => x.SerialNumber).IsUnique().HasFilter("\"serial_number\" IS NOT NULL");
    }
}