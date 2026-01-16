using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Inventories.Locations;

public sealed class StockLocation : Aggregate, ISoftDeletable, IHasMetadata
{
    public string Name { get; private set; } = string.Empty;
    public string Presentation { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;
    public bool IsDefault { get; private set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; private set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; private set; } = new Dictionary<string, object?>();

    private StockLocation() { }

    public static ErrorOr<StockLocation> Create(string name, string? presentation = null, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return StockLocationErrors.NameRequired;

        var location = new StockLocation
        {
            Name = name.Trim(),
            Presentation = presentation?.Trim() ?? name.Trim(),
            IsDefault = isDefault
        };

        location.RaiseDomainEvent(new StockLocationEvents.StockLocationCreated(location));

        return location;
    }

    public void Update(
        string name, 
        string presentation, 
        bool active,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        Name = name.Trim();
        Presentation = presentation.Trim();
        Active = active;

        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
        
        RaiseDomainEvent(new StockLocationEvents.StockLocationUpdated(this));
    }
}
