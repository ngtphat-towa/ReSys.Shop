using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Shared.Extensions;
using ErrorOr;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

public sealed class Taxon : Entity, IHasMetadata, IHasSlug
{
    public Guid TaxonomyId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Permalink { get; set; } = string.Empty;
    public int Position { get; set; }
    
    // Nested Set
    public int Lft { get; set; }
    public int Rgt { get; set; }
    public int Depth { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    // Relationships
    public Taxonomy Taxonomy { get; set; } = null!;
    public Taxon? Parent { get; set; }
    public ICollection<Taxon> Children { get; set; } = new List<Taxon>();

    private Taxon() { }

    public static ErrorOr<Taxon> Create(Guid taxonomyId, string name, Guid? parentId = null, string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonErrors.NameRequired;

        return new Taxon
        {
            TaxonomyId = taxonomyId,
            Name = name.Trim(),
            Presentation = name.Trim(),
            Slug = string.IsNullOrWhiteSpace(slug) ? name.ToSlug() : slug.ToSlug(),
            ParentId = parentId
        };
    }

    public ErrorOr<Success> Update(string name, string presentation, string? description, string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonErrors.NameRequired;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Description = description?.Trim();
        Slug = string.IsNullOrWhiteSpace(slug) ? name.ToSlug() : slug.ToSlug();

        return Result.Success;
    }

    public void UpdatePermalink(string taxonomyName)
    {
        var slugPath = Parent != null ? $"{Parent.Permalink}/{Slug}" : $"{taxonomyName.ToSlug()}/{Slug}";
        Permalink = slugPath.Trim('/');
    }

    public void SetParent(Guid? newParentId)
    {
        if (newParentId == Id) return;
        ParentId = newParentId;
    }
}
