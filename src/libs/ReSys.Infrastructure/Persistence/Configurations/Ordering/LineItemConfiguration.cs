using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class LineItemConfiguration : IEntityTypeConfiguration<LineItem>
{
    public void Configure(EntityTypeBuilder<LineItem> builder)
    {
        builder.ToTable(DatabaseTables.LineItems, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CapturedName)
            .HasMaxLength(LineItemConstraints.CapturedNameMaxLength)
            .IsRequired();

        builder.Property(x => x.CapturedSku)
            .HasMaxLength(LineItemConstraints.CapturedSkuMaxLength);

        builder.Property(x => x.Currency)
            .HasMaxLength(LineItemConstraints.CurrencyCodeLength)
            .IsRequired();

        builder.HasMany(x => x.Adjustments)
            .WithOne(x => x.LineItem)
            .HasForeignKey(x => x.LineItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InventoryUnits)
            .WithOne()
            .HasForeignKey(x => x.LineItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.VariantId);
    }
}