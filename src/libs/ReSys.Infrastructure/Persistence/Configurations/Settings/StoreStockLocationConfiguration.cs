using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Settings;

public class StoreStockLocationConfiguration : IEntityTypeConfiguration<StoreStockLocation>
{
    public void Configure(EntityTypeBuilder<StoreStockLocation> builder)
    {
        builder.ToTable(DatabaseTables.StoreStockLocations, DatabaseSchemas.Settings);

        // Composite Key
        builder.HasKey(x => new { x.StoreId, x.StockLocationId });

        // Properties
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Store)
            .WithMany(s => s.StoreStockLocations)
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StockLocation)
            .WithMany() // StockLocation doesn't know about Stores (Unidirectional from Store -> Location)
            .HasForeignKey(x => x.StockLocationId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting location if it's used by a store
    }
}
