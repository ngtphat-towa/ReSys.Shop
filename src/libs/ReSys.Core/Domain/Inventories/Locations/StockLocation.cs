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
    /// <summary>The display name of the location (e.g., 'Main Warehouse').</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The localized or descriptive name used for customer-facing documents.</summary>
    public string Presentation { get; set; } = string.Empty;

    /// <summary>The unique ERP-compatible alphanumeric identifier.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Indicates if the location is currently operational.</summary>
    public bool Active { get; set; } = true;

    /// <summary>The system-wide fallback location for inventory logic.</summary>
    public bool IsDefault { get; set; }

    /// <summary>The classification of the site (e.g., Warehouse, Retail, Transit).</summary>
    public StockLocationType Type { get; set; } = StockLocationType.Warehouse;

    /// <summary>Owned Entity: The verified physical address required for shipping and tax logic.</summary>
    public Address Address { get; set; } = null!;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Business Logic: Determines if the location is capable of shipping items to customers.
    /// Non-fulfillable locations (like 'Damaged' or 'Transit') are excluded from available stock counts.
    /// </summary>
    public bool IsFulfillable => Active && (Type == StockLocationType.Warehouse || Type == StockLocationType.RetailStore);

    public StockLocation() { }

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
        // Guard: Identification is mandatory
        if (string.IsNullOrWhiteSpace(name)) return StockLocationErrors.NameRequired;
        if (name.Length > StockLocationConstraints.NameMaxLength) return StockLocationErrors.NameTooLong;
        if (presentation.Length > StockLocationConstraints.PresentationMaxLength) return StockLocationErrors.PresentationTooLong;

        // Guard: Ensure ERP format compatibility
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

    /// <summary>
    /// Assigns the location as the primary node for stock logic.
    /// </summary>
    public void MarkAsDefault() => IsDefault = true;

    /// <summary>
    /// Removes the default status from the location.
    /// </summary>
    public void UnmarkAsDefault() => IsDefault = false;

    /// <summary>
    /// Resumes fulfillment capabilities for the site.
    /// </summary>
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
    /// Marks the location as logically removed while preserving history.
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
    /// Restores a soft-deleted location to operational status.
    /// </summary>
    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;

        IsDeleted = false;
        DeletedAt = null;
        RaiseDomainEvent(new StockLocationEvents.StockLocationRestored(this));
        return Result.Success;
    }

    /// <summary>
    /// Updates the logistical metadata for the location.
    /// </summary>
    public void SetMetadata(IDictionary<string, object?> publicMetadata, IDictionary<string, object?> privateMetadata)
    {
        PublicMetadata = publicMetadata;
        PrivateMetadata = privateMetadata;
    }
}