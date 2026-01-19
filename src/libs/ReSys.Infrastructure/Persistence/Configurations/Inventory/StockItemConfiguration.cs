using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Inventory;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable(DatabaseTables.StockItems, DatabaseSchemas.Catalog);
        builder.HasKey(x => x.Id);

        // Optimistic Concurrency
        builder.Property(x => x.Version).IsRowVersion();

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(StockItemConstraints.SkuMaxLength);

        builder.Property(x => x.QuantityOnHand).IsRequired();
        builder.Property(x => x.QuantityReserved).IsRequired();
        builder.Property(x => x.BackorderLimit).IsRequired();

        // Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.StockItems)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StockLocation)
            .WithMany()
            .HasForeignKey(x => x.StockLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.StockMovements)
            .WithOne(m => m.StockItem)
            .HasForeignKey(m => m.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InventoryUnits)
            .WithOne(u => u.StockItem)
            .HasForeignKey(u => u.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Sku, x.StockLocationId }).IsUnique();
        builder.HasIndex(x => x.VariantId);
    }
}
