using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Products;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable(DatabaseTables.ProductImages, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(ProductImageConstraints.UrlMaxLength);

        builder.Property(x => x.Alt)
            .HasMaxLength(ProductImageConstraints.AltMaxLength);

        builder.Property(x => x.Role)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .HasConversion<string>();

        // Embeddings - 1:N
        builder.OwnsMany(x => x.Embeddings, e =>
        {
            e.ToTable(DatabaseTables.ProductImageEmbeddings, DatabaseSchemas.Catalog);
            e.WithOwner().HasForeignKey(x => x.ProductImageId);
            e.HasKey(x => x.Id);
            
            e.Property(x => x.Vector)
                .HasColumnType("vector"); 
        });
    }
}
