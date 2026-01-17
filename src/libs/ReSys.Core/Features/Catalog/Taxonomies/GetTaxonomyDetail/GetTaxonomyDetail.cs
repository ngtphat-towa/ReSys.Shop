using ErrorOr;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Common;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.GetTaxonomyDetail;

public static class GetTaxonomyDetail
{
    // Request:
    public record Request(Guid Id);

    // Response:
    public record Response : TaxonomyDetail;

    // Query:
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var taxonomy = await context.Set<Taxonomy>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Request.Id, cancellationToken);

            if (taxonomy is null)
                return TaxonomyErrors.NotFound(query.Request.Id);

            var rootTaxons = await context.Set<Taxon>()
                .AsNoTracking()
                .Where(t => t.TaxonomyId == taxonomy.Id && t.Depth == 1)
                .OrderBy(t => t.Position)
                .Select(TaxonListItem.GetProjection<TaxonListItem>())
                .ToListAsync(cancellationToken);

            var response = taxonomy.Adapt<Response>();
            response.RootTaxons = rootTaxons;
            return response;
        }
    }
}