using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

public static class TaxonErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Taxon.NotFound",
        description: $"Taxon with ID '{id}' was not found.");

    public static Error NameRequired => Error.Validation(
        code: "Taxon.NameRequired",
        description: "Taxon name is required.");

    public static Error SelfParenting => Error.Validation(
        code: "Taxon.SelfParenting",
        description: "A taxon cannot be its own parent.");

    public static Error InvalidSlug => Error.Validation(
        code: "Taxon.InvalidSlug",
        description: "Slug contains invalid characters.");
}