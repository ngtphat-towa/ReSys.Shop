using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Products.Variants;

public static class VariantErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Variant.NotFound",
        description: $"Variant with ID '{id}' was not found.");

    public static Error SkuRequired => Error.Validation(
        code: "Variant.SkuRequired",
        description: "SKU is required.");

    public static Error SkuTooLong => Error.Validation(
        code: "Variant.SkuTooLong",
        description: $"SKU cannot exceed {VariantConstraints.SkuMaxLength} characters.");

    public static Error BarcodeTooLong => Error.Validation(
        code: "Variant.BarcodeTooLong",
        description: $"Barcode cannot exceed {VariantConstraints.BarcodeMaxLength} characters.");

    public static Error InvalidPrice => Error.Validation(
        code: "Variant.InvalidPrice",
        description: $"Price must be greater than or equal to {VariantConstraints.MinPrice}.");

    public static Error InvalidCompareAtPrice => Error.Validation(
        code: "Variant.InvalidCompareAtPrice",
        description: "Compare-at price must be greater than the current price.");

    public static Error InvalidDimension => Error.Validation(
        code: "Variant.InvalidDimension",
        description: $"Dimensions must be greater than or equal to {VariantConstraints.Dimensions.MinValue}.");

    public static Error MasterCannotHaveOptions => Error.Conflict(
        code: "Variant.MasterCannotHaveOptions",
        description: "Master variant cannot have specific option values.");
}
