using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.GetTaxonRules;

public static class GetTaxonRules
{
    public record Query(Guid TaxonomyId, Guid TaxonId) : IRequest<ErrorOr<List<TaxonRuleResponse>>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<List<TaxonRuleResponse>>>
    {
        public async Task<ErrorOr<List<TaxonRuleResponse>>> Handle(Query query, CancellationToken ct)
        {
            var rules = await context.Set<TaxonRule>()
                .AsNoTracking()
                .Where(r => r.TaxonId == query.TaxonId)
                .Select(TaxonRuleResponse.GetProjection<TaxonRuleResponse>())
                .ToListAsync(ct);

            return rules;
        }
    }
}