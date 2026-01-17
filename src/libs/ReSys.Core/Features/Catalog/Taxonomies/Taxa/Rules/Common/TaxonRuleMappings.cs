using System.Linq.Expressions;
using Mapster;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

public class TaxonRuleMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TaxonRule, TaxonRuleResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Value, src => src.Value)
            .Map(dest => dest.MatchPolicy, src => src.MatchPolicy)
            .Map(dest => dest.PropertyName, src => src.PropertyName);

        config.NewConfig<TaxonRule, AddTaxonRule.AddTaxonRule.Response>()
            .Inherits<TaxonRule, TaxonRuleResponse>();

        config.NewConfig<TaxonRule, UpdateTaxonRule.UpdateTaxonRule.Response>()
            .Inherits<TaxonRule, TaxonRuleResponse>();
    }

    public static Expression<Func<TaxonRule, T>> GetProjection<T>() where T : TaxonRuleResponse, new()
        => x => new T
        {
            Id = x.Id,
            Type = x.Type,
            Value = x.Value,
            MatchPolicy = x.MatchPolicy,
            PropertyName = x.PropertyName
        };
}
