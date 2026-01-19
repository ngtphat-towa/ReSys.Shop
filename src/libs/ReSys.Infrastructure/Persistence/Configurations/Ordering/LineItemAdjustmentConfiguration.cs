using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Ordering.Adjustments;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class LineItemAdjustmentConfiguration : IEntityTypeConfiguration<LineItemAdjustment>
{
    public void Configure(EntityTypeBuilder<LineItemAdjustment> builder)
    {
        builder.ToTable(DatabaseTables.LineItemAdjustments, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(AdjustmentConstraints.DescriptionMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.LineItemId);
        builder.HasIndex(x => x.PromotionId);
    }
}
