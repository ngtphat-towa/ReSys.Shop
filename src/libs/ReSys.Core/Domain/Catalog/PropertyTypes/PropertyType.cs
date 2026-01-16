using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Catalog.PropertyTypes;

public sealed class PropertyType : Aggregate, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool Filterable { get; set; }
    public PropertyKind Kind { get; set; } = PropertyKind.String;

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    private PropertyType() { }

    public static ErrorOr<PropertyType> Create(
        string name, 
        string? presentation = null, 
        PropertyKind kind = PropertyKind.String,
        int position = 0, 
        bool filterable = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return PropertyTypeErrors.NameRequired;

        var propertyType = new PropertyType
        {
            Name = name.Trim(),
            Presentation = presentation?.Trim() ?? name.Trim(),
            Kind = kind,
            Position = position,
            Filterable = filterable
        };

        propertyType.RaiseDomainEvent(new PropertyTypeEvents.PropertyTypeCreated(propertyType));
        return propertyType;
    }

    public ErrorOr<Success> Update(string name, string presentation, PropertyKind kind, int position, bool filterable)
    {
        if (string.IsNullOrWhiteSpace(name)) return PropertyTypeErrors.NameRequired;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Kind = kind;
        Position = position;
        Filterable = filterable;

        RaiseDomainEvent(new PropertyTypeEvents.PropertyTypeUpdated(this));
        return Result.Success;
    }

    public enum PropertyKind
    {
        String,
        Integer,
        Float,
        Boolean,
        Date,
        Html
    }
}