using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Testing.Examples;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations;

public class ExampleConfiguration : IEntityTypeConfiguration<Example>
{
    public void Configure(EntityTypeBuilder<Example> builder)
    {
        builder.ToTable(DatabaseTables.Examples, DatabaseSchemas.Testing);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(ExampleConstraints.NameMaxLength);

        builder.Property(e => e.Price)
            .HasPrecision(ExampleConstraints.PricePrecision, ExampleConstraints.PriceScale);

        builder.Property(e => e.HexColor)
            .HasMaxLength(ExampleConstraints.HexColorMaxLength);

        builder.HasIndex(e => e.Name).IsUnique();

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Examples)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}