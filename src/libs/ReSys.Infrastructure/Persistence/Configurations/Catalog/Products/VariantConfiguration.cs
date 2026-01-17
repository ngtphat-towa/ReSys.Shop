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
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CompareAtPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CostPrice)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(x => x.Weight).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Height).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Width).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Depth).HasColumnType("decimal(18,4)");

        // Relationships
        builder.HasMany(x => x.OptionValues)
            .WithMany(); // Uni-directional Many-to-Many
    }
}
