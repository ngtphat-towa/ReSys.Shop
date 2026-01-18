using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Features.Inventories.Locations.Common;

namespace ReSys.Core.Features.Inventories.Locations.GetStockLocationDetail;

public static class GetStockLocationDetail
{
    public record Request(Guid Id);
    public record Response : StockLocationDetail;
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var location = await context.Set<StockLocation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Request.Id && !x.IsDeleted, ct);

            if (location == null)
                return StockLocationErrors.NotFound(query.Request.Id);

            return location.Adapt<Response>();
        }
    }
}
