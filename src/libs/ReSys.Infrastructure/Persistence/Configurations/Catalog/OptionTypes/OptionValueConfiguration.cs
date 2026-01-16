using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.OptionTypes;

public class OptionValueConfiguration : IEntityTypeConfiguration<OptionValue>
{
    public void Configure(EntityTypeBuilder<OptionValue> builder)
    {
        builder.ToTable(DatabaseTables.OptionValues, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(OptionValueConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(OptionValueConstraints.PresentationMaxLength);

        builder.Property(x => x.Position);

        // Relationships
        builder.HasOne(x => x.OptionType)
            .WithMany(t => t.OptionValues)
            .HasForeignKey(x => x.OptionTypeId);
    }
}
