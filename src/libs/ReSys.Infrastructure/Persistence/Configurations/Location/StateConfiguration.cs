using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Location.States;
using ReSys.Core.Domain.Location.Countries;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Location;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable(DatabaseTables.States, DatabaseSchemas.Location);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(StateConstraints.NameMaxLength);

        builder.Property(x => x.Abbr)
            .HasMaxLength(StateConstraints.AbbrMaxLength);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CountryId);
    }
}
