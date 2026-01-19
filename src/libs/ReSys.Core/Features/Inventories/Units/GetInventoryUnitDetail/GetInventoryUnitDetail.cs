using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Features.Inventories.Units.Common;

namespace ReSys.Core.Features.Inventories.Units.GetInventoryUnitDetail;

public static class GetInventoryUnitDetail
{
    public record Request(Guid Id);
    public record Query(Request Request) : IRequest<ErrorOr<InventoryUnitDetail>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<InventoryUnitDetail>>
    {
        public async Task<ErrorOr<InventoryUnitDetail>> Handle(Query query, CancellationToken ct)
        {
            var unit = await context.Set<InventoryUnit>()
                .Include(x => x.StockItem!)
                    .ThenInclude(s => s.StockLocation)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Request.Id && !x.IsDeleted, ct);

            if (unit == null)
                return InventoryUnitErrors.NotFound(query.Request.Id);

            return unit.Adapt<InventoryUnitDetail>();
        }
    }
}
