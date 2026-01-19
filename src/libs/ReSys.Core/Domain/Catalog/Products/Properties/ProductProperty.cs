using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.PropertyTypes;

namespace ReSys.Core.Domain.Catalog.Products.Properties;

/// <summary>
/// Assigns a specific property value to a product (e.g., Material: Cotton).
/// Glue entity between Product and PropertyType.
/// </summary>
public sealed class ProductProperty : Entity
{
    public Guid ProductId { get; set; }
    public Guid PropertyTypeId { get; set; }
    public string Value { get; set; } = string.Empty;

    // Relationships
    public PropertyType PropertyType { get; set; } = null!;

    public ProductProperty() { }

    /// <summary>
    /// Factory for creating a product specification entry.
    /// </summary>
    public static ProductProperty Create(Guid productId, Guid propertyTypeId, string value)
    {
        return new ProductProperty
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            PropertyTypeId = propertyTypeId,
            Value = value.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Updates the specification value.
    /// </summary>
    public void UpdateValue(string newValue)
    {
        Value = newValue.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
