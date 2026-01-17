using System.Linq.Expressions;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

/// <summary>
/// Core parameters for taxon rules.
/// </summary>
public record TaxonRuleParameters
{
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string MatchPolicy { get; set; } = null!;
    public string? PropertyName { get; set; }
}

/// <summary>
/// Input model for creating or updating a taxon rule.
/// </summary>
public record TaxonRuleInput : TaxonRuleParameters
{
    public Guid? Id { get; set; }
}

/// <summary>
/// Response model for taxon rule data.
/// </summary>
public record TaxonRuleResponse : TaxonRuleParameters
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Manual Projection for high-performance rule queries.
    /// </summary>
    public static Expression<Func<TaxonRule, T>> GetProjection<T>() where T : TaxonRuleResponse, new()
        => x => new T
        {
            Id = x.Id,
            Type = x.Type,
            Value = x.Value,
            MatchPolicy = x.MatchPolicy,
            PropertyName = x.PropertyName,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
}