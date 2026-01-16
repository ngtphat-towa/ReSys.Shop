using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Catalog.OptionTypes;

public class OptionTypeConfiguration : IEntityTypeConfiguration<OptionType>
{
    public void Configure(EntityTypeBuilder<OptionType> builder)
    {
        builder.ToTable(DatabaseTables.OptionTypes, DatabaseSchemas.Catalog);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(OptionTypeConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .IsRequired()
            .HasMaxLength(OptionTypeConstraints.PresentationMaxLength);
        
        builder.Property(x => x.Position);

        // Relationships
        builder.HasMany(x => x.OptionValues)
            .WithOne(v => v.OptionType)
            .HasForeignKey(v => v.OptionTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
