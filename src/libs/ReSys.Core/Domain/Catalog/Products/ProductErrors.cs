using ErrorOr;
using ReSys.Core.Domain.Catalog.Products.Variants;

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
    
    public static Error PresentationTooLong => Error.Validation(
        code: "Product.PresentationTooLong",
        description: $"Product presentation cannot exceed {ProductConstraints.PresentationMaxLength} characters.");
    
    public static Error DescriptionTooLong => Error.Validation(
        code: "Product.DescriptionTooLong",
        description: $"Product description cannot exceed {ProductConstraints.DescriptionMaxLength} characters.");

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
        
        public static Error MetaKeywordsTooLong => Error.Validation(
            code: "Product.Seo.MetaKeywordsTooLong",
            description: $"Meta keywords cannot exceed {ProductConstraints.Seo.MetaKeywordsMaxLength} characters.");
    }

    public static Error DuplicateName => Error.Conflict(
        code: "Product.DuplicateName",
        description: "A product with the same name already exists.");
    
    public static Error DuplicateSku => Error.Conflict(
        code: "Product.DuplicateSku",
        description: "A variant with the same SKU already exists.");

    public static Error CannotDeleteMasterVariant => Error.Conflict(
        code: "Product.CannotDeleteMasterVariant",
        description: "The master variant of a product cannot be deleted.");

    public static Error InvalidDiscontinuedOn => Error.Validation(
        code: "Product.InvalidDiscontinuedOn",
        description: "Discontinued date must be after or equal to make active date.");

    public static Error DuplicateClassification => Error.Conflict(
        code: "Product.DuplicateClassification",
        description: "Product is already classified under this taxon.");

    public static Error ClassificationNotFound => Error.NotFound(
        code: "Product.ClassificationNotFound",
        description: "Classification for the specified taxon was not found.");

    public static class Status
    {
        public static Error AlreadyActive => Error.Conflict(
            code: "Product.Status.AlreadyActive",
            description: "Product is already active.");

        public static Error AlreadyArchived => Error.Conflict(
            code: "Product.Status.AlreadyArchived",
            description: "Product is already archived.");
    }
}
