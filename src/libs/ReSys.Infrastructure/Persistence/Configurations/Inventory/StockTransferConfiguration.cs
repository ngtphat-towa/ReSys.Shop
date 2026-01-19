using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Inventory;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable(DatabaseTables.StockTransfers, DatabaseSchemas.Catalog);
        builder.HasKey(x => x.Id);

        // Optimistic Concurrency
        builder.Property(x => x.Version).IsRowVersion();

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(StockTransferConstraints.ReferenceNumberMaxLength);

        // Uses Postgres Native Enum
        builder.Property(x => x.Status);

        builder.Property(x => x.Reason)
            .HasMaxLength(StockTransferConstraints.ReasonMaxLength);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ReferenceNumber).IsUnique();
        builder.HasIndex(x => x.SourceLocationId);
        builder.HasIndex(x => x.DestinationLocationId);
    }
}

public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.ToTable(DatabaseTables.StockTransferItems, DatabaseSchemas.Catalog);
        builder.HasKey(x => x.Id);

        builder.HasOne<Core.Domain.Catalog.Products.Variants.Variant>()
            .WithMany()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StockTransferId).IsRequired();
        builder.HasIndex(x => x.StockTransferId);
    }
}
