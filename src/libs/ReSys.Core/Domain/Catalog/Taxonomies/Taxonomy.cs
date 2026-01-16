using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.Domain.Catalog.Taxonomies;

public sealed class Taxonomy : Aggregate, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public int Position { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public ICollection<Taxon> Taxons { get; set; } = new List<Taxon>();

    private Taxonomy() { }

    public static ErrorOr<Taxonomy> Create(string name, string? presentation = null, int position = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonomyErrors.NameRequired;

        var taxonomy = new Taxonomy
        {
            Name = name.Trim(),
            Presentation = presentation?.Trim() ?? name.Trim(),
            Position = position
        };

        taxonomy.RaiseDomainEvent(new TaxonomyEvents.TaxonomyCreated(taxonomy));
        return taxonomy;
    }

    public ErrorOr<Taxon> AddTaxon(string name, Guid? parentId = null)
    {
        if (parentId.HasValue && !Taxons.Any(t => t.Id == parentId.Value))
            return TaxonErrors.NotFound(parentId.Value);

        var taxonResult = Taxon.Create(Id, name, parentId);
        if (taxonResult.IsError) return taxonResult.Errors;

        Taxons.Add(taxonResult.Value);
        return taxonResult.Value;
    }

    public void RemoveTaxon(Guid taxonId)
    {
        var taxon = Taxons.FirstOrDefault(t => t.Id == taxonId);
        if (taxon == null) return;

        // Recursive removal of children if they are in the flat collection
        var children = Taxons.Where(t => t.ParentId == taxonId).ToList();
        foreach (var child in children)
        {
            RemoveTaxon(child.Id);
        }

        Taxons.Remove(taxon);
    }
}
