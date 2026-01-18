using ErrorOr;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Common.Constraints;

namespace ReSys.Core.Domain.Catalog.Taxonomies;

public static class TaxonomyErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Taxonomy.NotFound",
        description: $"Taxonomy with ID '{id}' was not found.");

    public static Error NameRequired => CommonErrors.Validation.Required("Taxonomy.Name");
    public static Error NameTooLong => CommonErrors.Validation.TooLong("Taxonomy.Name", CommonConstraints.NameMaxLength);

    public static Error PresentationRequired => CommonErrors.Validation.Required("Taxonomy.Presentation");
    public static Error PresentationTooLong => CommonErrors.Validation.TooLong("Taxonomy.Presentation", CommonConstraints.PresentationMaxLength);

    public static Error DuplicateName => Error.Conflict(
        code: "Taxonomy.DuplicateName",
        description: "A taxonomy with the same name already exists.");

    public static Error HasTaxons => Error.Validation(
        code: "Taxonomy.HasTaxons",
        description: "Cannot delete a taxonomy with associated taxons. Delete or move all taxons first.");

    public static Error InvalidPosition => Error.Validation(
        code: "Taxonomy.InvalidPosition",
        description: $"Position must be greater than or equal to {TaxonomyConstraints.MinPosition}.");
}