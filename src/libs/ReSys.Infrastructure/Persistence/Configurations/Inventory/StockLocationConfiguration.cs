using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Inventories.Locations;
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
            a.Property(p => p.Address1).HasMaxLength(255).IsRequired();
            a.Property(p => p.Address2).HasMaxLength(255);
            a.Property(p => p.City).HasMaxLength(100).IsRequired();
            a.Property(p => p.ZipCode).HasMaxLength(20).IsRequired();
            a.Property(p => p.CountryCode).HasMaxLength(2).IsRequired();
            a.Property(p => p.StateCode).HasMaxLength(5);
            a.Property(p => p.Phone).HasMaxLength(50);
            a.Property(p => p.FirstName).HasMaxLength(100);
            a.Property(p => p.LastName).HasMaxLength(100);
            a.Property(p => p.Company).HasMaxLength(255);
        });
    }
}