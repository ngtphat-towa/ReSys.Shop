using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Common;

/// <summary>
/// Provides manual mapping for Taxon domain entities to DTOs.
/// This ensures expression safety and consistent performance.
/// </summary>
public static class TaxonMappings
{
    public static T MapToListItem<T>(this Taxon source) where T : TaxonListItem, new()
    {
        return new T
        {
            Id = source.Id,
            TaxonomyId = source.TaxonomyId,
            ParentId = source.ParentId,
            Name = source.Name,
            Presentation = source.Presentation,
            Description = source.Description,
            Slug = source.Slug,
            Permalink = source.Permalink,
            PrettyName = source.PrettyName,
            Position = source.Position,
            HideFromNav = source.HideFromNav,
            ImageUrl = source.ImageUrl,
            SquareImageUrl = source.SquareImageUrl,
            Depth = source.Depth,
            Lft = source.Lft,
            Rgt = source.Rgt,
            ProductCount = source.Classifications?.Count ?? 0,
            ChildCount = source.Children?.Count ?? 0,
            HasChildren = source.Rgt - source.Lft > 1,
            Automatic = source.Automatic
        };
    }

    public static T MapToDetail<T>(this Taxon source) where T : TaxonDetail, new()
    {
        var detail = MapToListItem<T>(source);

        detail.RulesMatchPolicy = source.RulesMatchPolicy;
        detail.SortOrder = source.SortOrder;
        detail.MetaTitle = source.MetaTitle;
        detail.MetaDescription = source.MetaDescription;
        detail.MetaKeywords = source.MetaKeywords;
        detail.PublicMetadata = new Dictionary<string, object?>(source.PublicMetadata);
        detail.PrivateMetadata = new Dictionary<string, object?>(source.PrivateMetadata);

        return detail;
    }

    public static TaxonSelectListItem MapToSelectListItem(this Taxon source)
    {
        return new TaxonSelectListItem
        {
            Id = source.Id,
            Name = source.Name,
            Presentation = source.Presentation
        };
    }
}
