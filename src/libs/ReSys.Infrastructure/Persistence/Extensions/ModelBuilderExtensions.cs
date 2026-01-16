using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using ReSys.Core.Domain.Common.Abstractions;

using System.Text.Json;

namespace ReSys.Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies configuration for all entities implementing IAuditable.
    /// </summary>
    public static ModelBuilder ApplyAuditableConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType != null && typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                // Use Metadata API to avoid re-configuring Owned Types as non-owned
                var createdAt = entityType.FindProperty(nameof(IAuditable.CreatedAt))
                                ?? entityType.AddProperty(nameof(IAuditable.CreatedAt), typeof(DateTime));
                createdAt.IsNullable = false;

                // Ensure CreatedBy exists
                if (entityType.FindProperty(nameof(IAuditable.CreatedBy)) == null)
                {
                    entityType.AddProperty(nameof(IAuditable.CreatedBy), typeof(string));
                }

                // Ensure UpdatedAt exists
                if (entityType.FindProperty(nameof(IAuditable.UpdatedAt)) == null)
                {
                    entityType.AddProperty(nameof(IAuditable.UpdatedAt), typeof(DateTime?));
                }

                // Ensure UpdatedBy exists
                if (entityType.FindProperty(nameof(IAuditable.UpdatedBy)) == null)
                {
                    entityType.AddProperty(nameof(IAuditable.UpdatedBy), typeof(string));
                }
            }
        }
        return modelBuilder;
    }

    /// <summary>
    /// Applies configuration for all entities implementing IHasMetadata.
    /// Sets column type to 'jsonb' for Postgres.
    /// </summary>
    public static ModelBuilder ApplyMetadataConfiguration(this ModelBuilder modelBuilder)
    {
        // Define converter and comparer for JSON serialization
        var jsonOptions = new JsonSerializerOptions();

        var dictionaryConverter = new ValueConverter<IDictionary<string, object?>, string>(
            v => JsonSerializer.Serialize(v, jsonOptions),
            v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, jsonOptions) ?? new Dictionary<string, object?>()
        );

        var dictionaryComparer = new ValueComparer<IDictionary<string, object?>>(
            (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
            c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
            c => JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType != null && typeof(IHasMetadata).IsAssignableFrom(entityType.ClrType))
            {
                // Use Metadata API
                // Get the actual property type from the CLR class to avoid mismatch errors (e.g. IDictionary vs Dictionary)
                var publicMetaPropInfo = entityType.ClrType.GetProperty(nameof(IHasMetadata.PublicMetadata));
                var publicMetaType = publicMetaPropInfo?.PropertyType ?? typeof(Dictionary<string, object?>);

                var pubMeta = entityType.FindProperty(nameof(IHasMetadata.PublicMetadata))
                              ?? entityType.AddProperty(nameof(IHasMetadata.PublicMetadata), publicMetaType);

                pubMeta.SetColumnType("jsonb");
                pubMeta.SetValueConverter(dictionaryConverter);
                pubMeta.SetValueComparer(dictionaryComparer);

                var privateMetaPropInfo = entityType.ClrType.GetProperty(nameof(IHasMetadata.PrivateMetadata));
                var privateMetaType = privateMetaPropInfo?.PropertyType ?? typeof(Dictionary<string, object?>);

                var privMeta = entityType.FindProperty(nameof(IHasMetadata.PrivateMetadata))
                               ?? entityType.AddProperty(nameof(IHasMetadata.PrivateMetadata), privateMetaType);

                privMeta.SetColumnType("jsonb");
                privMeta.SetValueConverter(dictionaryConverter);
                privMeta.SetValueComparer(dictionaryComparer);
            }
        }
        return modelBuilder;
    }
}
