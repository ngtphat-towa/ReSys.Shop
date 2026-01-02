using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;

namespace ReSys.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(ProductConstraints.NameMaxLength);
            
        builder.Property(e => e.Price)
            .HasPrecision(ProductConstraints.PricePrecision, ProductConstraints.PriceScale);

        builder.HasIndex(e => e.Name).IsUnique();
    }
}