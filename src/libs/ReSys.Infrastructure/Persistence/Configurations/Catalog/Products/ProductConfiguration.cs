using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.Products;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(DatabaseTables.Products, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ProductConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(ProductConstraints.NameMaxLength); // Assuming same as Name

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(ProductConstraints.SlugMaxLength);

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(ProductConstraints.Seo.MetaTitleMaxLength);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(ProductConstraints.Seo.MetaDescriptionMaxLength);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(ProductConstraints.Seo.MetaKeywordsMaxLength);

        // Relationships
        builder.HasMany(x => x.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProductProperties)
            .WithOne()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many OptionTypes
        builder.HasMany(p => p.OptionTypes)
            .WithMany(); // If there's no navigation back, simple join table is created. 
                         // Check OptionType.cs if it has navigation back.
    }
}
