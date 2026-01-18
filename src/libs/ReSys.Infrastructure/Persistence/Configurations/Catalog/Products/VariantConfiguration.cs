using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Products;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.ToTable(DatabaseTables.Variants, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku)
            .HasMaxLength(VariantConstraints.SkuMaxLength);
        
        builder.HasIndex(x => x.Sku)
            .IsUnique()
            .HasFilter("sku IS NOT NULL"); // Only enforce uniqueness for non-null SKUs

        builder.Property(x => x.Barcode)
            .HasMaxLength(VariantConstraints.BarcodeMaxLength);

        builder.Property(x => x.Price)
            .HasPrecision(VariantConstraints.PricePrecision, VariantConstraints.PriceScale);

        builder.Property(x => x.CompareAtPrice)
            .HasPrecision(VariantConstraints.PricePrecision, VariantConstraints.PriceScale);

        builder.Property(x => x.CostPrice)
            .HasPrecision(VariantConstraints.PricePrecision, VariantConstraints.PriceScale);
        
        builder.Property(x => x.Weight).HasPrecision(VariantConstraints.DimensionPrecision, VariantConstraints.DimensionScale);
        builder.Property(x => x.Height).HasPrecision(VariantConstraints.DimensionPrecision, VariantConstraints.DimensionScale);
        builder.Property(x => x.Width).HasPrecision(VariantConstraints.DimensionPrecision, VariantConstraints.DimensionScale);
        builder.Property(x => x.Depth).HasPrecision(VariantConstraints.DimensionPrecision, VariantConstraints.DimensionScale);

        // Relationships
        builder.HasMany(x => x.OptionValues)
            .WithMany(); // Uni-directional Many-to-Many
    }
}
