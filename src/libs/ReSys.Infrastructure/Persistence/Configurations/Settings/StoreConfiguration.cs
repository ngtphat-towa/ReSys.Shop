using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Settings;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable(DatabaseTables.Stores, DatabaseSchemas.Settings);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(StoreConstraints.NameMaxLength);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(StoreConstraints.CodeMaxLength);

        builder.Property(x => x.DefaultCurrency)
            .IsRequired()
            .HasMaxLength(StoreConstraints.CurrencyCodeMaxLength);

        builder.Property(x => x.Url)
            .HasMaxLength(StoreConstraints.UrlMaxLength);

        // Optimistic Concurrency
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique();

        // Explicit Join Entity: Store -> StoreStockLocations
        builder.HasMany(x => x.StoreStockLocations)
            .WithOne(x => x.Store)
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
