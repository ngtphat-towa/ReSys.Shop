using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.PropertyTypes;

namespace ReSys.Core.Domain.Catalog.Products;

public sealed class ProductProperty : Entity
{
    public Guid ProductId { get; set; }
    public Guid PropertyTypeId { get; set; }
    public string Value { get; set; } = string.Empty;

    // Relationships
    public PropertyType PropertyType { get; set; } = null!;

    private ProductProperty() { }

    public static ProductProperty Create(Guid productId, Guid propertyTypeId, string value)
    {
        return new ProductProperty
        {
            ProductId = productId,
            PropertyTypeId = propertyTypeId,
            Value = value.Trim()
        };
    }

    public void UpdateValue(string newValue)
    {
        Value = newValue.Trim();
    }
}
