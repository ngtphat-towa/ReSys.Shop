using ErrorOr;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

public static class TaxonErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Taxon.NotFound",
        description: $"Taxon with ID '{id}' was not found.");

    public static Error SelfParenting => Error.Validation(
        code: "Taxon.SelfParenting",
        description: "A taxon cannot be its own parent.");

    public static Error CircularParenting => Error.Validation(
        code: "Taxon.CircularParenting",
        description: "Circular parenting detected. A taxon cannot be a descendant of itself.");

    public static Error InvalidParent => Error.Validation(
        code: "Taxon.InvalidParent",
        description: "The specified parent taxon does not exist in the same taxonomy.");

    public static Error HasChildren => Error.Conflict(
        code: "Taxon.HasChildren",
        description: "Cannot delete a taxon that has children.");

    public static Error NameRequired => CommonErrors.Validation.Required("Taxon.Name");
    public static Error NameTooLong => CommonErrors.Validation.TooLong("Taxon.Name", CommonConstraints.NameMaxLength);

    public static Error PresentationRequired => CommonErrors.Validation.Required("Taxon.Presentation");
    public static Error PresentationTooLong => CommonErrors.Validation.TooLong("Taxon.Presentation", CommonConstraints.PresentationMaxLength);

    public static Error DuplicateName => Error.Conflict(
        code: "Taxon.DuplicateName",
        description: "A taxon with the same name already exists in this level of the taxonomy.");

    public static Error SlugRequired => CommonErrors.Validation.Required("Taxon.Slug");
    public static Error SlugTooLong => CommonErrors.Validation.TooLong("Taxon.Slug", CommonConstraints.SlugMaxLength);
    public static Error InvalidSlug => Error.Validation(
        code: "Taxon.InvalidSlug",
        description: "Slug contains invalid characters.");

    public static Error InvalidRulesMatchPolicy => Error.Validation(
        code: "Taxon.InvalidRulesMatchPolicy",
        description: "Rules match policy must be 'all' or 'any'.");

    public static Error SortOrderRequired => Error.Validation(
        code: "Taxon.SortOrderRequired",
        description: "Sort order is required when automatic classification is enabled.");

    public static Error InvalidPosition => Error.Validation(
        code: "Taxon.InvalidPosition",
        description: "Position must be greater than or equal to 0.");

    public static Error ParentTaxonomyMismatch => Error.Validation(
        code: "Taxon.ParentTaxonomyMismatch",
        description: "The parent taxon must belong to the same taxonomy.");

    public static Error RootLock => Error.Validation(
        code: "Taxon.RootLock",
        description: "Root taxon cannot be moved, deleted, or re-parented.");

    public static Error CycleDetected => Error.Validation(
        code: "Taxon.CycleDetected",
        description: "Cycle detected in hierarchy. A taxon cannot be a descendant of itself.");

    public static Error NoRoot => Error.NotFound(
        code: "Taxon.NoRoot",
        description: "No root taxon found for the specified taxonomy.");

    public static Error RootConflict => Error.Conflict(
        code: "Taxon.RootConflict",
        description: "Taxonomy has multiple root taxons.");

    public static Error HierarchyRebuildFailed => Error.Failure(
        code: "Taxon.HierarchyRebuildFailed",
        description: "Failed to rebuild taxonomy hierarchy.");

    public static Error RegenerationFailed => Error.Failure(
        code: "Taxon.RegenerationFailed",
        description: "Failed to regenerate products for taxon.");
}