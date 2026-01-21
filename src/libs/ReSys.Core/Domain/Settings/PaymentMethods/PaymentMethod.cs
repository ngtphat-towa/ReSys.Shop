using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Settings.PaymentMethods;

public sealed class PaymentMethod : Aggregate, ISoftDeletable, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Active;
    public int Position { get; set; }
    public bool AutoCapture { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public PaymentMethod() { }

    public static ErrorOr<PaymentMethod> Create(
        string name,
        string presentation,
        PaymentType type,
        string? description = null,
        int position = 0,
        bool autoCapture = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return PaymentMethodErrors.NameRequired;
        if (name.Length > PaymentMethodConstraints.NameMaxLength) return PaymentMethodErrors.NameTooLong;
        if (presentation.Length > PaymentMethodConstraints.PresentationMaxLength) return PaymentMethodErrors.PresentationTooLong;
        if (description?.Length > PaymentMethodConstraints.DescriptionMaxLength) return PaymentMethodErrors.DescriptionTooLong;

        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Presentation = presentation.Trim(),
            Description = description?.Trim(),
            Type = type,
            Position = position,
            AutoCapture = autoCapture,
            Status = PaymentStatus.Active
        };

        method.RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodCreated(method));
        return method;
    }

    public ErrorOr<Success> UpdateDetails(
        string name,
        string presentation,
        string? description,
        int position,
        bool autoCapture)
    {
        if (string.IsNullOrWhiteSpace(name)) return PaymentMethodErrors.NameRequired;
        if (name.Length > PaymentMethodConstraints.NameMaxLength) return PaymentMethodErrors.NameTooLong;
        if (presentation.Length > PaymentMethodConstraints.PresentationMaxLength) return PaymentMethodErrors.PresentationTooLong;
        if (description?.Length > PaymentMethodConstraints.DescriptionMaxLength) return PaymentMethodErrors.DescriptionTooLong;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Description = description?.Trim();
        Position = position;
        AutoCapture = autoCapture;

        RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodUpdated(this));
        return Result.Success;
    }

    public void Activate()
    {
        if (Status == PaymentStatus.Active) return;
        Status = PaymentStatus.Active;
        RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodActivated(this));
    }

    public void Deactivate()
    {
        if (Status == PaymentStatus.Inactive) return;
        Status = PaymentStatus.Inactive;
        RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodDeactivated(this));
    }

    public void Archive()
    {
        if (Status == PaymentStatus.Archived) return;
        Status = PaymentStatus.Archived;
        RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodUpdated(this)); // Or specific Archived event
    }

    public ErrorOr<Deleted> Delete()
    {
        if (IsDeleted) return Result.Deleted;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodDeleted(this));
        return Result.Deleted;
    }

    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted) return Result.Success;
        IsDeleted = false;
        DeletedAt = null;
        RaiseDomainEvent(new PaymentMethodEvents.PaymentMethodRestored(this));
        return Result.Success;
    }

    public void SetMetadata(IDictionary<string, object?>? publicMetadata, IDictionary<string, object?>? privateMetadata)
    {
        if (publicMetadata != null) PublicMetadata = new Dictionary<string, object?>(publicMetadata);
        if (privateMetadata != null) PrivateMetadata = new Dictionary<string, object?>(privateMetadata);
    }

    public enum PaymentType
    {
        CreditCard,
        DebitCard,
        BankTransfer,
        CashOnDelivery,
        PayPal,
        Stripe,
        StoreCredit,
        GiftCard
    }

    public enum PaymentStatus { Inactive, Active, Archived }
}