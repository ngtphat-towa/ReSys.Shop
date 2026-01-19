using System.Linq.Expressions;

using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Common;

/// <summary>
/// Common query options for filtering and searching taxons.
/// </summary>
public record TaxonQueryOptions : QueryOptions
{
    public Guid[]? TaxonomyId { get; set; }
    public Guid? FocusedTaxonId { get; set; }
    public bool? IncludeLeavesOnly { get; set; }
    public bool IncludeHidden { get; set; } = false;
    public int? MaxDepth { get; set; }
}

/// <summary>
/// Base parameters for taxon data entry and display.
/// </summary>
public record TaxonParameters : IHasPosition
{
    public string Name { get; set; } = null!;
    public string Presentation { get; set; } = null!;
    public string? Description { get; set; }
    public string Slug { get; set; } = null!;
    public int Position { get; set; }
    public bool HideFromNav { get; set; }
    public string? ImageUrl { get; set; }
    public string? SquareImageUrl { get; set; }
}

/// <summary>
/// Detailed input model for creating or updating a taxon.
/// </summary>
public record TaxonInput : TaxonParameters, IHasMetadata
{
    public Guid? ParentId { get; set; }
    public bool Automatic { get; set; }
    public string RulesMatchPolicy { get; set; } = "all";
    public string SortOrder { get; set; } = "manual";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

/// <summary>
/// Light-weight model for taxon lists.
/// </summary>
public record TaxonListItem : TaxonParameters
{
    public Guid Id { get; set; }
    public Guid TaxonomyId { get; set; }
    public Guid? ParentId { get; set; }
    public string Permalink { get; set; } = null!;
    public string PrettyName { get; set; } = null!;
    public int Depth { get; set; }
    public int Lft { get; set; }
    public int Rgt { get; set; }
    public int ProductCount { get; set; }
    public int ChildCount { get; set; }
    public bool HasChildren { get; set; }
    public bool Automatic { get; set; }

    /// <summary>
    /// Manual Projection for high-performance tree/paged queries.
    /// </summary>
    public static Expression<Func<Taxon, T>> GetProjection<T>() where T : TaxonListItem, new()
        => x => new T
        {
            Id = x.Id,
            TaxonomyId = x.TaxonomyId,
            ParentId = x.ParentId,
            Name = x.Name,
            Presentation = x.Presentation,
            Description = x.Description,
            Slug = x.Slug,
            Permalink = x.Permalink,
            PrettyName = x.PrettyName,
            Position = x.Position,
            HideFromNav = x.HideFromNav,
            ImageUrl = x.ImageUrl,
            SquareImageUrl = x.SquareImageUrl,
            Depth = x.Depth,
            Lft = x.Lft,
            Rgt = x.Rgt,
            ProductCount = x.Classifications.Count,
            ChildCount = x.Children.Count,
            HasChildren = x.Rgt - x.Lft > 1,
            Automatic = x.Automatic
        };
}

/// <summary>
/// Model for simple selection lists.
/// </summary>
public record TaxonSelectListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Presentation { get; set; } = null!;

    public static Expression<Func<Taxon, T>> GetProjection<T>() where T : TaxonSelectListItem, new()
        => x => new T
        {
            Id = x.Id,
            Name = x.Name,
            Presentation = x.Presentation
        };
}

/// <summary>
/// Recursive model for hierarchical taxon tree display.
/// </summary>
public record TaxonTreeItem : TaxonListItem
{
    public bool IsExpanded { get; set; }
    public bool IsInActivePath { get; set; }
    public IEnumerable<TaxonTreeItem> Children { get; set; } = [];
}

/// <summary>
/// Response wrapper for taxon tree requests.
/// </summary>
public record TaxonTreeResponse
{
    public IEnumerable<TaxonTreeItem> Tree { get; set; } = [];
    public IEnumerable<TaxonTreeItem> Breadcrumbs { get; set; } = [];
    public TaxonTreeItem? FocusedNode { get; set; }
    public TaxonTreeItem? FocusedSubtree { get; set; }
}

/// <summary>
/// Full detail model for a single taxon.
/// </summary>
public record TaxonDetail : TaxonListItem, IHasMetadata
{
    public string RulesMatchPolicy { get; set; } = "all";
    public string SortOrder { get; set; } = "manual";
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// Manual Projection for high-performance detail queries.
    /// </summary>
    public static Expression<Func<Taxon, T>> GetDetailProjection<T>() where T : TaxonDetail, new()
        => x => new T
        {
            Id = x.Id,
            TaxonomyId = x.TaxonomyId,
            ParentId = x.ParentId,
            Name = x.Name,
            Presentation = x.Presentation,
            Description = x.Description,
            Slug = x.Slug,
            Permalink = x.Permalink,
            PrettyName = x.PrettyName,
            Position = x.Position,
            HideFromNav = x.HideFromNav,
            ImageUrl = x.ImageUrl,
            SquareImageUrl = x.SquareImageUrl,
            Depth = x.Depth,
            Lft = x.Lft,
            Rgt = x.Rgt,
            ProductCount = x.Classifications.Count,
            ChildCount = x.Children.Count,
            HasChildren = x.Rgt - x.Lft > 1,
            Automatic = x.Automatic,
            RulesMatchPolicy = x.RulesMatchPolicy,
            SortOrder = x.SortOrder,
            MetaTitle = x.MetaTitle,
            MetaDescription = x.MetaDescription,
            MetaKeywords = x.MetaKeywords,
            PublicMetadata = x.PublicMetadata,
            PrivateMetadata = x.PrivateMetadata
        };
}
