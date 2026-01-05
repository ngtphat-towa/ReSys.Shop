using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;

namespace ReSys.Infrastructure.Persistence.Configurations;

public class ExampleConfiguration : IEntityTypeConfiguration<Example>
{
    public void Configure(EntityTypeBuilder<Example> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(ExampleConstraints.NameMaxLength);

        builder.Property(e => e.Price)
            .HasPrecision(ExampleConstraints.PricePrecision, ExampleConstraints.PriceScale);

        builder.Property(e => e.HexColor)
            .HasMaxLength(ExampleConstraints.HexColorMaxLength);

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();

        // Relationships
        
        // Ignores
    }
}