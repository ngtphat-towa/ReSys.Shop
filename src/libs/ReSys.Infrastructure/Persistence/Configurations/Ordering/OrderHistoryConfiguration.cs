using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ReSys.Core.Domain.Ordering.History;
using ReSys.Shared.Constants;
using System.Text.Json;

namespace ReSys.Infrastructure.Persistence.Configurations.Ordering;

public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
{
    public void Configure(EntityTypeBuilder<OrderHistory> builder)
    {
        builder.ToTable(DatabaseTables.OrderHistories, DatabaseSchemas.Ordering);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(OrderHistoryConstraints.DescriptionMaxLength)
            .IsRequired();

        builder.Property(x => x.TriggeredBy)
            .HasMaxLength(OrderHistoryConstraints.TriggeredByMaxLength);

        var jsonOptions = new JsonSerializerOptions();
        var converter = new ValueConverter<IDictionary<string, object?>, string>(
            v => JsonSerializer.Serialize(v, jsonOptions),
            v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, jsonOptions) ?? new Dictionary<string, object?>()
        );

        builder.Property(x => x.Context)
            .HasColumnType("jsonb")
            .HasConversion(converter);

        builder.HasIndex(x => x.OrderId);
    }
}
