using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Product.NotFound",
        description: $"Product with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Product.NameRequired",
        description: "Product name is required.");

    public static Error NameTooLong => Error.Validation(
        code: "Product.NameTooLong",
        description: $"Product name cannot exceed {ProductConstraints.NameMaxLength} characters.");

    public static Error InvalidSlug => Error.Validation(
        code: "Product.InvalidSlug",
        description: "Slug must be a URL-friendly string containing only lowercase letters, numbers, and hyphens.");

    public static class Seo
    {
        public static Error MetaTitleTooLong => Error.Validation(
            code: "Product.Seo.MetaTitleTooLong",
            description: $"Meta title cannot exceed {ProductConstraints.Seo.MetaTitleMaxLength} characters.");

        public static Error MetaDescriptionTooLong => Error.Validation(
            code: "Product.Seo.MetaDescriptionTooLong",
            description: $"Meta description cannot exceed {ProductConstraints.Seo.MetaDescriptionMaxLength} characters.");
    }
}