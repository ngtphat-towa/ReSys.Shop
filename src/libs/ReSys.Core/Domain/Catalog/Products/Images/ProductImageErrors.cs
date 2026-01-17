using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Products.Images;

public static class ProductImageErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "ProductImage.NotFound",
        description: $"Product image with ID '{id}' was not found.");

    public static Error UrlRequired => Error.Validation(
        code: "ProductImage.UrlRequired",
        description: "Image URL is required.");

    public static Error UrlTooLong => Error.Validation(
        code: "ProductImage.UrlTooLong",
        description: $"Image URL must not exceed {ProductImageConstraints.UrlMaxLength} characters.");
}
