using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Settings.ShippingMethods;

public sealed class ShippingMethod : Aggregate, ISoftDeletable, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ShippingType Type { get; set; }
    public decimal BaseCost { get; set; }
    public ShippingStatus Status { get; set; } = ShippingStatus.Active;
    public int Position { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public ShippingMethod() { }

    public static ErrorOr<ShippingMethod> Create(
        string name,
        string presentation,
        ShippingType type,
        decimal baseCost,
        string? description = null,
        int position = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) return ShippingMethodErrors.NameRequired;
        if (name.Length > ShippingMethodConstraints.NameMaxLength) return ShippingMethodErrors.NameTooLong;
        if (presentation.Length > ShippingMethodConstraints.PresentationMaxLength) return ShippingMethodErrors.PresentationTooLong;
        if (description?.Length > ShippingMethodConstraints.DescriptionMaxLength) return ShippingMethodErrors.DescriptionTooLong;
        if (baseCost < ShippingMethodConstraints.MinCost) return ShippingMethodErrors.CostNegative;

        var method = new ShippingMethod
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Presentation = presentation.Trim(),
            Type = type,
            BaseCost = baseCost,
            Description = description?.Trim(),
            Position = position,
            Status = ShippingStatus.Active
        };

        method.RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodCreated(method));
        return method;
    }

    public ErrorOr<Success> UpdateDetails(
        string name,
        string presentation,
        decimal baseCost,
        string? description,
        int position)
    {
        if (string.IsNullOrWhiteSpace(name)) return ShippingMethodErrors.NameRequired;
        if (name.Length > ShippingMethodConstraints.NameMaxLength) return ShippingMethodErrors.NameTooLong;
        if (presentation.Length > ShippingMethodConstraints.PresentationMaxLength) return ShippingMethodErrors.PresentationTooLong;
        if (description?.Length > ShippingMethodConstraints.DescriptionMaxLength) return ShippingMethodErrors.DescriptionTooLong;
        if (baseCost < ShippingMethodConstraints.MinCost) return ShippingMethodErrors.CostNegative;

        Name = name.Trim();
        Presentation = presentation.Trim();
        BaseCost = baseCost;
        Description = description?.Trim();
        Position = position;

        RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodUpdated(this));
        return Result.Success;
    }

    public void Activate()
    {
        if (Status == ShippingStatus.Active) return;
        Status = ShippingStatus.Active;
        RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodActivated(this));
    }

    public void Deactivate()
    {
        if (Status == ShippingStatus.Inactive) return;
        Status = ShippingStatus.Inactive;
        RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodDeactivated(this));
    }

    public void Archive()
    {
        if (Status == ShippingStatus.Archived) return;
        Status = ShippingStatus.Archived;
        RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodUpdated(this));
    }

    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodDeleted(this));
        return Result.Deleted;
    }

    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;
        IsDeleted = false;
        DeletedAt = null;
        RaiseDomainEvent(new ShippingMethodEvents.ShippingMethodRestored(this));
        return Result.Success;
    }

    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }

    public enum ShippingType
    {
        Standard,
        Express,
        Overnight,
        Pickup,
        FreeShipping
    }

    public enum ShippingStatus { Inactive, Active, Archived }
}