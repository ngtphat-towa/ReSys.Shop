using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Settings.ShippingMethods;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Settings;

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable(DatabaseTables.ShippingMethods, DatabaseSchemas.System);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ShippingMethodConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(ShippingMethodConstraints.NameMaxLength);

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.Property(x => x.BaseCost)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.Position);

        // Metadata is handled globally in AppDbContext via ApplyMetadataConfiguration
    }
}
