using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Shared.Extensions;

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

    public Taxon? RootTaxon => Taxons.FirstOrDefault(t => t.ParentId == null);

    private Taxonomy() { }

    public static ErrorOr<Taxonomy> Create(string name, string? presentation = null, int position = TaxonomyConstraints.DefaultPosition)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonomyErrors.NameRequired;
        if (name.Length > TaxonomyConstraints.NameMaxLength) return TaxonomyErrors.NameTooLong;

        var finalPresentation = presentation?.Trim() ?? name.Trim();
        if (finalPresentation.Length > TaxonomyConstraints.PresentationMaxLength) return TaxonomyErrors.PresentationTooLong;

        if (position < TaxonomyConstraints.MinPosition) return TaxonomyErrors.InvalidPosition;

        var taxonomy = new Taxonomy
        {
            Name = name.Trim(),
            Presentation = finalPresentation,
            Position = position
        };

        // Create the initial root taxon matching taxonomy name
        var rootTaxonResult = Taxon.Create(taxonomy.Id, taxonomy.Name);
        if (rootTaxonResult.IsError) return rootTaxonResult.Errors;

        var rootTaxon = rootTaxonResult.Value;
        rootTaxon.UpdatePermalink(taxonomy.Name);
        taxonomy.Taxons.Add(rootTaxon);

        taxonomy.RaiseDomainEvent(new TaxonomyEvents.TaxonomyCreated(taxonomy));
        return taxonomy;
    }

    public ErrorOr<Success> Update(string name, string presentation, int position)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonomyErrors.NameRequired;
        if (name.Length > TaxonomyConstraints.NameMaxLength) return TaxonomyErrors.NameTooLong;

        if (string.IsNullOrWhiteSpace(presentation)) return TaxonomyErrors.PresentationRequired;
        if (presentation.Length > TaxonomyConstraints.PresentationMaxLength) return TaxonomyErrors.PresentationTooLong;

        if (position < TaxonomyConstraints.MinPosition) return TaxonomyErrors.InvalidPosition;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Position = position;

        // Synchronize Root Taxon Name and Slug
        var root = RootTaxon;
        if (root != null && (root.Name != Name || root.Presentation != Presentation))
        {
            root.Update(Name, Presentation, root.Description, Name.ToSlug());
            root.UpdatePermalink(Name);
        }

        RaiseDomainEvent(new TaxonomyEvents.TaxonomyUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        // Only allow deletion if only root taxon exists and it has no children
        var root = RootTaxon;
        if (root != null && root.Children.Any())
        {
            return TaxonomyErrors.HasTaxons;
        }

        if (Taxons.Count > 1)
            return TaxonomyErrors.HasTaxons;

        RaiseDomainEvent(new TaxonomyEvents.TaxonomyDeleted(this));
        return Result.Deleted;
    }

    #region Taxon Management

    public ErrorOr<Taxon> AddTaxon(string name, Guid? parentId = null)
    {
        // Enforce: Only 1 root allowed. If parentId is null, use existing root as parent.
        var effectiveParentId = parentId;
        var root = RootTaxon;

        if (effectiveParentId == null && root != null)
        {
            effectiveParentId = root.Id;
        }

        // Enforce Invariant: Unique name per level
        if (Taxons.Any(t => t.ParentId == effectiveParentId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return TaxonErrors.DuplicateName;
        }

        if (effectiveParentId.HasValue && !Taxons.Any(t => t.Id == effectiveParentId.Value))
            return TaxonErrors.NotFound(effectiveParentId.Value);

        var taxonResult = Taxon.Create(Id, name, effectiveParentId);
        if (taxonResult.IsError) return taxonResult.Errors;

        var taxon = taxonResult.Value;
        Taxons.Add(taxon);

        RaiseDomainEvent(new TaxonEvents.TaxonCreated(taxon));
        return taxon;
    }

    #endregion
}
