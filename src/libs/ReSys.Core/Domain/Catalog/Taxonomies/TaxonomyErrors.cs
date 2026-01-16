using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Taxonomies;

public static class TaxonomyErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Taxonomy.NotFound",
        description: $"Taxonomy with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Taxonomy.NameRequired",
        description: "Name is required.");

    public static Error HasTaxons => Error.Validation(
        code: "Taxonomy.HasTaxons",
        description: "Cannot delete a taxonomy with associated taxons. Delete or move all taxons first.");
}
