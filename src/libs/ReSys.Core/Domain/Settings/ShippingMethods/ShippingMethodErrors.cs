using ErrorOr;

namespace ReSys.Core.Domain.Settings.ShippingMethods;

public static class ShippingMethodErrors
{
    // Business Errors
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "ShippingMethod.NotFound",
        description: $"Shipping method with ID '{id}' was not found.");

    public static Error InUse => Error.Conflict(
        code: "ShippingMethod.InUse",
        description: "Cannot delete shipping method that is in use by active orders.");

    public static Error AlreadyActive => Error.Conflict(
        code: "ShippingMethod.AlreadyActive",
        description: "Shipping method is already active.");

    public static Error AlreadyInactive => Error.Conflict(
        code: "ShippingMethod.AlreadyInactive",
        description: "Shipping method is already inactive.");

    public static Error CannotDeactivateDefault => Error.Conflict(
        code: "ShippingMethod.CannotDeactivateDefault",
        description: "Default shipping method cannot be deactivated.");

    public static Error CannotDeleteDefault => Error.Conflict(
        code: "ShippingMethod.CannotDeleteDefault",
        description: "Default shipping method cannot be deleted.");

    // Validation Errors
    public static Error NameRequired => Error.Validation(
        code: "ShippingMethod.NameRequired",
        description: "Shipping method name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "ShippingMethod.NameTooLong",
        description: $"Shipping method name cannot exceed {ShippingMethodConstraints.NameMaxLength} characters.");

    public static Error PresentationTooLong => Error.Validation(
        code: "ShippingMethod.PresentationTooLong",
        description: $"Shipping method presentation cannot exceed {ShippingMethodConstraints.PresentationMaxLength} characters.");

    public static Error DescriptionTooLong => Error.Validation(
        code: "ShippingMethod.DescriptionTooLong",
        description: $"Shipping method description cannot exceed {ShippingMethodConstraints.DescriptionMaxLength} characters.");

    public static Error CostNegative => Error.Validation(
        code: "ShippingMethod.CostNegative",
        description: "Cost cannot be negative.");
}