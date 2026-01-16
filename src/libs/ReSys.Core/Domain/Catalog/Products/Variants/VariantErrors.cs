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

    public static Error InvalidPrice => Error.Validation(
        code: "Variant.InvalidPrice",
        description: "Price must be greater than or equal to 0.");
}
