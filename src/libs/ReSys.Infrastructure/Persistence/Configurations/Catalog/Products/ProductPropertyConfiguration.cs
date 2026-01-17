using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Products.Properties;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Products;

public class ProductPropertyConfiguration : IEntityTypeConfiguration<ProductProperty>
{
    public void Configure(EntityTypeBuilder<ProductProperty> builder)
    {
        builder.ToTable(DatabaseTables.ProductProperties, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value).IsRequired();

        // Relationships
        builder.HasOne(x => x.PropertyType)
            .WithMany()
            .HasForeignKey(x => x.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
