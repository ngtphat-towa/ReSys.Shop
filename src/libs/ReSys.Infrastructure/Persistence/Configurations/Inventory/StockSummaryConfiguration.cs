using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Inventory;

public class StockSummaryConfiguration : IEntityTypeConfiguration<StockSummary>
{
    public void Configure(EntityTypeBuilder<StockSummary> builder)
    {
        // Placed in Catalog schema because it is a Commercial Projection
        builder.ToTable("stock_summaries", DatabaseSchemas.Catalog);
        
        builder.HasKey(x => x.Id);

        // One-to-One with Variant (Shared Primary Key)
        builder.HasOne<Core.Domain.Catalog.Products.Variants.Variant>()
            .WithOne()
            .HasForeignKey<StockSummary>(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.TotalOnHand).IsRequired();
        builder.Property(x => x.TotalReserved).IsRequired();
        builder.Property(x => x.TotalAvailable).IsRequired();
        builder.Property(x => x.IsBuyable).IsRequired();
        
        builder.HasIndex(x => x.VariantId).IsUnique();
        builder.HasIndex(x => x.IsBuyable);
    }
}
