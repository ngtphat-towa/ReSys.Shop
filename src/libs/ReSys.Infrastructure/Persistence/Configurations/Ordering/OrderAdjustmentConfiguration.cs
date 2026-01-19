using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Ordering.Adjustments;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class OrderAdjustmentConfiguration : IEntityTypeConfiguration<OrderAdjustment>
{
    public void Configure(EntityTypeBuilder<OrderAdjustment> builder)
    {
        builder.ToTable(DatabaseTables.OrderAdjustments, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(AdjustmentConstraints.DescriptionMaxLength)
            .IsRequired();

        builder.Property(x => x.Scope);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.PromotionId);
    }
}
