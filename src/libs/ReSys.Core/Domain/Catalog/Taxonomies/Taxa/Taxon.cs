using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Shared.Extensions;
using ErrorOr;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

public sealed class Taxon : Aggregate, IHasMetadata, IHasSlug
{
    #region Properties
    public Guid TaxonomyId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Permalink { get; set; } = string.Empty;
    public string PrettyName { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool HideFromNav { get; set; }

    // Nested Set
    public int Lft { get; set; }
    public int Rgt { get; set; }
    public int Depth { get; set; }

    // Automatic Classification
    public bool Automatic { get; set; }
    public string RulesMatchPolicy { get; set; } = "all"; // all, any
    public string SortOrder { get; set; } = "manual";
    public bool MarkedForRegenerateProducts { get; set; }
    public bool MarkedForRegenerateTaxonProducts { get; set; }

    // Images
    public string? ImageUrl { get; set; }
    public string? SquareImageUrl { get; set; }

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
    public ICollection<TaxonRule> TaxonRules { get; set; } = new List<TaxonRule>();
    public ICollection<Classification> Classifications { get; set; } = new List<Classification>();
    #endregion

    private Taxon() { }

    public static ErrorOr<Taxon> Create(
        Guid taxonomyId,
        string name,
        Guid? parentId = null,
        string? slug = null,
        bool automatic = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonErrors.NameRequired;
        if (name.Length > TaxonConstraints.NameMaxLength) return TaxonErrors.NameTooLong;

        var taxon = new Taxon
        {
            TaxonomyId = taxonomyId,
            Name = name.Trim(),
            Presentation = name.Trim(),
            Slug = string.IsNullOrWhiteSpace(slug) ? name.ToSlug() : slug.ToSlug(),
            ParentId = parentId,
            Automatic = automatic
        };

        taxon.RaiseDomainEvent(new TaxonEvents.TaxonCreated(taxon));

        return taxon;
    }

    public ErrorOr<Success> Update(
        string name,
        string presentation,
        string? description,
        string? slug = null,
        bool? automatic = null,
        bool? hideFromNav = null,
        string? rulesMatchPolicy = null,
        string? sortOrder = null,
        string? imageUrl = null,
        string? squareImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return TaxonErrors.NameRequired;
        if (name.Length > TaxonConstraints.NameMaxLength) return TaxonErrors.NameTooLong;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Description = description?.Trim();
        Slug = string.IsNullOrWhiteSpace(slug) ? name.ToSlug() : slug.ToSlug();

        if (automatic.HasValue && automatic.Value != Automatic)
        {
            Automatic = automatic.Value;
            MarkedForRegenerateProducts = true;
        }

        if (hideFromNav.HasValue) HideFromNav = hideFromNav.Value;
        if (!string.IsNullOrWhiteSpace(rulesMatchPolicy)) RulesMatchPolicy = rulesMatchPolicy;
        if (!string.IsNullOrWhiteSpace(sortOrder)) SortOrder = sortOrder;

        if (imageUrl != null) ImageUrl = imageUrl;
        if (squareImageUrl != null) SquareImageUrl = squareImageUrl;

        RaiseDomainEvent(new TaxonEvents.TaxonUpdated(this));
        return Result.Success;
    }

    #region Rule Management

    public ErrorOr<TaxonRule> AddRule(string type, string value, string? matchPolicy = null, string? propertyName = null)
    {
        // Enforce Invariant: Unique rule combination
        if (TaxonRules.Any(r => r.Type == type && r.Value == value && r.MatchPolicy == matchPolicy && r.PropertyName == propertyName))
            return TaxonRuleErrors.Duplicate;

        var ruleResult = TaxonRule.Create(Id, type, value, matchPolicy, propertyName);
        if (ruleResult.IsError) return ruleResult.Errors;

        var rule = ruleResult.Value;
        TaxonRules.Add(rule);
        
        MarkedForRegenerateProducts = true;
        RaiseDomainEvent(new TaxonRuleEvents.TaxonRuleAdded(this, rule));
        return rule;
    }

    public ErrorOr<Success> UpdateRule(Guid ruleId, string type, string value, string? matchPolicy = null, string? propertyName = null)
    {
        var rule = TaxonRules.FirstOrDefault(r => r.Id == ruleId);
        if (rule == null) return TaxonRuleErrors.NotFound(ruleId);

        var result = rule.Update(type, value, matchPolicy, propertyName);
        if (result.IsError) return result.Errors;

        MarkedForRegenerateProducts = true;
        RaiseDomainEvent(new TaxonRuleEvents.TaxonRuleUpdated(this, rule));
        return Result.Success;
    }

    public ErrorOr<Success> RemoveRule(Guid ruleId)
    {
        var rule = TaxonRules.FirstOrDefault(r => r.Id == ruleId);
        if (rule == null) return TaxonRuleErrors.NotFound(ruleId);

        TaxonRules.Remove(rule);
        MarkedForRegenerateProducts = true;
        RaiseDomainEvent(new TaxonRuleEvents.TaxonRuleRemoved(this, rule));
        return Result.Success;
    }

    #endregion

    public void UpdatePermalink(string taxonomyName)
    {
        var slugPath = Parent != null ? $"{Parent.Permalink}/{Slug}" : taxonomyName.ToSlug();
        Permalink = slugPath.Trim('/');

        var prettyPath = Parent != null ? $"{Parent.PrettyName} -> {Presentation}" : Presentation;
        PrettyName = prettyPath;
    }

    public ErrorOr<Success> SetParent(Guid? newParentId)
    {
        if (ParentId == null) return TaxonErrors.RootLock;
        if (newParentId == Id) return TaxonErrors.SelfParenting;

        var oldParentId = ParentId;
        ParentId = newParentId;

        RaiseDomainEvent(new TaxonEvents.TaxonMoved(this, oldParentId, newParentId));
        return Result.Success;
    }

    public void SetPosition(int position)
    {
        if (Position != position)
        {
            Position = position;
            RaiseDomainEvent(new TaxonEvents.TaxonMoved(this, ParentId, ParentId));
        }
    }

    public ErrorOr<Deleted> Delete()
    {
        if (ParentId == null) return TaxonErrors.RootLock;
        if (Children.Any()) return TaxonErrors.HasChildren;

        RaiseDomainEvent(new TaxonEvents.TaxonDeleted(this));
        return Result.Deleted;
    }
}