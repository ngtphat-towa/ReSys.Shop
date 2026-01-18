using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using System.Text.RegularExpressions;
using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Core.Domain.Inventories.Locations;

/// <summary>
/// Represents a physical or logical storage site for inventory.
/// Acts as a source for order fulfillment or a destination for returns and transfers.
/// </summary>
public sealed class StockLocation : Aggregate, ISoftDeletable, IHasMetadata
{
    public string Name { get; private set; } = string.Empty;
    public string Presentation { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;
    public bool IsDefault { get; private set; }
    public StockLocationType Type { get; private set; } = StockLocationType.Warehouse;

    /// <summary>Owned Entity: The verified physical address required for shipping and tax logic.</summary>
    public Address Address { get; private set; } = null!;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; private set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; private set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Business Logic: Determines if the location is capable of shipping items to customers.
    /// Non-fulfillable locations (like 'Damaged' or 'Transit') are excluded from available stock counts.
    /// </summary>
    public bool IsFulfillable => Active && (Type == StockLocationType.Warehouse || Type == StockLocationType.RetailStore);

    private StockLocation() { }

    /// <summary>
    /// Creates a new physical storage location with a unique ERP-compatible code.
    /// </summary>
    public static ErrorOr<StockLocation> Create(
        string name,
        string code,
        Address address,
        string? presentation = null,
        bool isDefault = false,
        StockLocationType type = StockLocationType.Warehouse)
    {
        // Guard: Identification is mandatory
        if (string.IsNullOrWhiteSpace(name)) return StockLocationErrors.NameRequired;
        if (string.IsNullOrWhiteSpace(code)) return StockLocationErrors.CodeRequired;

        // Guard: Logistics requires a physical destination
        if (address == null) return AddressErrors.Address1Required;

        // Guard: Ensure ERP format compatibility
        var normalizedCode = code.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(normalizedCode, StockLocationConstraints.CodeRegex))
            return StockLocationErrors.InvalidCodeFormat;

        var location = new StockLocation
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Presentation = presentation?.Trim() ?? name.Trim(),
            Code = normalizedCode,
            Address = address,
            IsDefault = isDefault,
            Type = type,
            Active = true
        };

        location.RaiseDomainEvent(new StockLocationEvents.StockLocationCreated(location));
        return location;
    }

    /// <summary>
    /// Updates core location identity and logistics metadata.
    /// </summary>
    public ErrorOr<Success> Update(
        string name,
        string presentation,
        string code,
        bool isDefault,
        StockLocationType type,
        Address address)
    {
        if (string.IsNullOrWhiteSpace(name)) return StockLocationErrors.NameRequired;
        if (string.IsNullOrWhiteSpace(code)) return StockLocationErrors.CodeRequired;
        if (address == null) return AddressErrors.Address1Required;

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(normalizedCode, StockLocationConstraints.CodeRegex))
            return StockLocationErrors.InvalidCodeFormat;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Code = normalizedCode;
        IsDefault = isDefault;
        Type = type;
        Address = address;

        RaiseDomainEvent(new StockLocationEvents.StockLocationUpdated(this));
        return Result.Success;
    }

    public void MarkAsDefault() => IsDefault = true;
    public void UnmarkAsDefault() => IsDefault = false;

    public void Activate()
    {
        if (Active) return;
        Active = true;
        RaiseDomainEvent(new StockLocationEvents.StockLocationActivated(this));
    }

    /// <summary>
    /// Disables the location for fulfillment.
    /// </summary>
    public ErrorOr<Success> Deactivate()
    {
        // Guard: Prevent deactivating the "Anchor" location to avoid system-wide fulfillment failure
        if (!Active) return Result.Success;
        if (IsDefault) return StockLocationErrors.CannotDeactivateDefault;

        Active = false;
        RaiseDomainEvent(new StockLocationEvents.StockLocationDeactivated(this));
        return Result.Success;
    }

    /// <summary>
    /// Logical deletion.
    /// </summary>
    public ErrorOr<Deleted> Delete()
    {
        // Guard: The default location must always exist for fallback logic
        if (IsDeleted) return Result.Deleted;
        if (IsDefault) return StockLocationErrors.CannotDeleteDefault;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new StockLocationEvents.StockLocationDeleted(this));
        return Result.Deleted;
    }

    /// <summary>
    /// Restores a logically deleted location.
    /// </summary>
    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;

        IsDeleted = false;
        DeletedAt = null;
        RaiseDomainEvent(new StockLocationEvents.StockLocationRestored(this));
        return Result.Success;
    }
}