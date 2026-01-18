using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Inventory;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable(DatabaseTables.StockMovements, DatabaseSchemas.Catalog);
        builder.HasKey(x => x.Id);

        // Uses Postgres Native Enum
        builder.Property(x => x.Type);

        builder.Property(x => x.Reason)
            .HasMaxLength(StockItemConstraints.Movements.MaxReasonLength);

        builder.Property(x => x.Reference)
            .HasMaxLength(StockItemConstraints.Movements.MaxReferenceLength);

        builder.Property(x => x.BalanceBefore).IsRequired();
        builder.Property(x => x.BalanceAfter).IsRequired();
        
        // Use precision from VariantConstraints
        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.StockItem)
            .WithMany(s => s.StockMovements)
            .HasForeignKey(x => x.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => x.StockItemId);
        builder.HasIndex(x => x.Reference);
    }
}
