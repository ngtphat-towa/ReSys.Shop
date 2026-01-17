using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Shared.Constants;
using Pgvector;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Products;

public class ImageEmbeddingConfiguration : IEntityTypeConfiguration<ImageEmbedding>
{
    public void Configure(EntityTypeBuilder<ImageEmbedding> builder)
    {
        builder.ToTable(DatabaseTables.ProductImageEmbeddings, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(ProductImageConstraints.ModelNameMaxLength);

        builder.Property(x => x.ModelVersion)
            .IsRequired()
            .HasMaxLength(ProductImageConstraints.ModelVersionMaxLength);

        // Native PostgreSQL type configuration
        // Note: Global configuration in AppDbContext handles ignoring this for non-Postgres providers
        builder.Property(x => x.Vector)
            .HasColumnType("vector")
            .HasConversion(
                v => v.ToArray(),
                v => new Vector(v));

        // Relationship is primarily managed in ProductImageConfiguration
        builder.HasOne(x => x.ProductImage)
            .WithMany(x => x.Embeddings)
            .HasForeignKey(x => x.ProductImageId);
    }
}
