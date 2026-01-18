using System.ComponentModel;
using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Catalog.PropertyTypes;

public enum PropertyKind
{
    [Description("String")]
    String,
    [Description("Integer")]
    Integer,
    [Description("Float")]
    Float,
    [Description("Boolean")]
    Boolean,
    [Description("Date")]
    Date,
    [Description("HTML")]
    Html
}

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
        int position = PropertyTypeConstraints.DefaultPosition, 
        bool filterable = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return PropertyTypeErrors.NameRequired;
        if (name.Length > PropertyTypeConstraints.NameMaxLength) return PropertyTypeErrors.NameTooLong;

        var finalPresentation = presentation?.Trim() ?? name.Trim();
        if (finalPresentation.Length > PropertyTypeConstraints.PresentationMaxLength) return PropertyTypeErrors.PresentationTooLong;

        if (position < PropertyTypeConstraints.MinPosition) return PropertyTypeErrors.InvalidPosition;

        var propertyType = new PropertyType
        {
            Name = name.Trim(),
            Presentation = finalPresentation,
            Kind = kind,
            Position = position,
            Filterable = filterable
        };

        propertyType.RaiseDomainEvent(new PropertyTypeEvents.PropertyTypeCreated(propertyType));
        return propertyType;
    }

    public ErrorOr<Success> Update(
        string name, 
        string presentation, 
        PropertyKind kind, 
        int position, 
        bool filterable)
    {
        if (string.IsNullOrWhiteSpace(name)) return PropertyTypeErrors.NameRequired;
        if (name.Length > PropertyTypeConstraints.NameMaxLength) return PropertyTypeErrors.NameTooLong;

        if (string.IsNullOrWhiteSpace(presentation)) return PropertyTypeErrors.PresentationRequired;
        if (presentation.Length > PropertyTypeConstraints.PresentationMaxLength) return PropertyTypeErrors.PresentationTooLong;

        if (position < PropertyTypeConstraints.MinPosition) return PropertyTypeErrors.InvalidPosition;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Kind = kind;
        Position = position;
        Filterable = filterable;

        RaiseDomainEvent(new PropertyTypeEvents.PropertyTypeUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        RaiseDomainEvent(new PropertyTypeEvents.PropertyTypeDeleted(this));
        return Result.Deleted;
    }
}
