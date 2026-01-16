using ErrorOr;

namespace ReSys.Core.Domain.Settings.ShippingMethods;

public static class ShippingMethodErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "ShippingMethod.NotFound",
        description: $"Shipping method with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "ShippingMethod.NameRequired",
        description: "Name is required.");

    public static Error CostNegative => Error.Validation(
        code: "ShippingMethod.CostNegative",
        description: "Cost cannot be negative.");
}
