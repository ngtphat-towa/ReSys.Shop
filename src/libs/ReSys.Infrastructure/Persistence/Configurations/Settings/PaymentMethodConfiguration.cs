using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Settings;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable(DatabaseTables.PaymentMethods, DatabaseSchemas.System);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(PaymentMethodConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(PaymentMethodConstraints.NameMaxLength);

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .HasConversion<string>();

        builder.HasIndex(x => x.Position);

        // Metadata is handled globally in AppDbContext via ApplyMetadataConfiguration
    }
}
