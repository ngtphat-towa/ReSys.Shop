using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Inventory;

public class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        builder.ToTable(DatabaseTables.StockLocations, DatabaseSchemas.Catalog);
        builder.HasKey(x => x.Id);

        // Optimistic Concurrency
        builder.Property(x => x.Version).IsRowVersion();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(StockLocationConstraints.NameMaxLength);

        builder.Property(x => x.Presentation)
            .HasMaxLength(StockLocationConstraints.PresentationMaxLength);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(StockLocationConstraints.CodeMaxLength);

        // Uses Postgres Native Enum defined in AppDbContext
        builder.Property(x => x.Type);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Code).IsUnique();

        // Map Address as Owned Entity
        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Address1).HasMaxLength(AddressConstraints.Address1MaxLength).IsRequired();
            a.Property(p => p.Address2).HasMaxLength(AddressConstraints.Address2MaxLength);
            a.Property(p => p.City).HasMaxLength(AddressConstraints.CityMaxLength).IsRequired();
            a.Property(p => p.ZipCode).HasMaxLength(AddressConstraints.ZipCodeMaxLength).IsRequired();
            a.Property(p => p.CountryCode).HasMaxLength(AddressConstraints.CountryCodeLength).IsRequired();
            a.Property(p => p.StateCode).HasMaxLength(AddressConstraints.StateCodeMaxLength);
            a.Property(p => p.Phone).HasMaxLength(AddressConstraints.PhoneMaxLength);
            a.Property(p => p.FirstName).HasMaxLength(AddressConstraints.NameMaxLength);
            a.Property(p => p.LastName).HasMaxLength(AddressConstraints.NameMaxLength);
            a.Property(p => p.Company).HasMaxLength(AddressConstraints.CompanyMaxLength);
        });
    }
}