using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.GetTaxonDetail;

public static class GetTaxonDetail
{
    // Request:
    public record Request(Guid Id);

    // Response:
    public record Response : TaxonDetail;

    // Query:
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var response = await context.Set<Taxon>()
                .AsNoTracking()
                .Where(x => x.Id == query.Request.Id)
                .Select(TaxonDetail.GetDetailProjection<Response>())
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
                return TaxonErrors.NotFound(query.Request.Id);

            return response;
        }
    }
}
