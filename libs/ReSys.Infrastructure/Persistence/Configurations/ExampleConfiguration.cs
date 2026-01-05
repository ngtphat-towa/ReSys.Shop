using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain;
using ReSys.Core.Features.Examples.Common;

namespace ReSys.Infrastructure.Persistence.Configurations;

public class ExampleConfiguration : IEntityTypeConfiguration<Example>
{
    public void Configure(EntityTypeBuilder<Example> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(ExampleConstraints.NameMaxLength);
            
        builder.Property(e => e.Price)
            .HasPrecision(ExampleConstraints.PricePrecision, ExampleConstraints.PriceScale);

        builder.Property(e => e.HexColor)
            .HasMaxLength(ExampleConstraints.HexColorMaxLength);

        builder.HasIndex(e => e.Name).IsUnique();
    }
}