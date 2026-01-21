using ErrorOr;

namespace ReSys.Core.Domain.Settings.PaymentMethods;

public static class PaymentMethodErrors
{
    // Business Errors
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "PaymentMethod.NotFound",
        description: $"Payment method with ID '{id}' was not found.");

    public static Error InUse => Error.Conflict(
        code: "PaymentMethod.InUse",
        description: "Cannot delete payment method that is in use by active orders.");

    public static Error AlreadyActive => Error.Conflict(
        code: "PaymentMethod.AlreadyActive",
        description: "Payment method is already active.");

    public static Error AlreadyInactive => Error.Conflict(
        code: "PaymentMethod.AlreadyInactive",
        description: "Payment method is already inactive.");

    // Validation Errors
    public static Error NameRequired => Error.Validation(
        code: "PaymentMethod.NameRequired",
        description: "Payment method name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "PaymentMethod.NameTooLong",
        description: $"Payment method name cannot exceed {PaymentMethodConstraints.NameMaxLength} characters.");

    public static Error PresentationTooLong => Error.Validation(
        code: "PaymentMethod.PresentationTooLong",
        description: $"Payment method presentation cannot exceed {PaymentMethodConstraints.PresentationMaxLength} characters.");

    public static Error DescriptionTooLong => Error.Validation(
        code: "PaymentMethod.DescriptionTooLong",
        description: $"Payment method description cannot exceed {PaymentMethodConstraints.DescriptionMaxLength} characters.");
}