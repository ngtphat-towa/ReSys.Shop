using ErrorOr;

namespace ReSys.Core.Features.Products.Common;

public static class ProductErrors
{
    public static Error DuplicateName => Error.Conflict(
        code: "Product.DuplicateName",
        description: "A product with the same name already exists.");

    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Product.NotFound",
        description: $"The product with ID '{id}' was not found.");

    public static Error InvalidPrice => Error.Validation(
        code: "Product.InvalidPrice",
        description: $"Price must be greater than or equal to {ProductConstraints.MinPrice}.");

    public static Error NameTooLong => Error.Validation(
        code: "Product.NameTooLong",
        description: $"Product name cannot exceed {ProductConstraints.NameMaxLength} characters.");

    public static Error NameRequired => Error.Validation(
        code: "Product.NameRequired",
        description: "Product name is required.");

    public static Error DescriptionRequired => Error.Validation(
        code: "Product.DescriptionRequired",
        description: "Product description is required.");
}