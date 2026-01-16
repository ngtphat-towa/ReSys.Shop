using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Location.Countries;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Location;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable(DatabaseTables.Countries, DatabaseSchemas.Location);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(CountryConstraints.NameMaxLength);

        builder.Property(x => x.Iso)
            .IsRequired()
            .HasMaxLength(CountryConstraints.IsoMaxLength)
            .IsFixedLength();

        builder.Property(x => x.Iso3)
            .HasMaxLength(CountryConstraints.Iso3MaxLength)
            .IsFixedLength();

        builder.HasIndex(x => x.Iso).IsUnique();
    }
}
