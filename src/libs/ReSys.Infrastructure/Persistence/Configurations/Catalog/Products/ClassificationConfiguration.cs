using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Products;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Products;

public class ClassificationConfiguration : IEntityTypeConfiguration<Classification>
{
    public void Configure(EntityTypeBuilder<Classification> builder)
    {
        builder.ToTable(DatabaseTables.Classifications, DatabaseSchemas.Catalog);

        builder.HasKey(x => new { x.ProductId, x.TaxonId });

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Classifications)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Taxon)
            .WithMany(t => t.Classifications)
            .HasForeignKey(x => x.TaxonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Position).HasDefaultValue(0);
    }
}