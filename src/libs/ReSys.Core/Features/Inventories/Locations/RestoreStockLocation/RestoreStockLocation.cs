using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Inventories.Locations.RestoreStockLocation;

public static class RestoreStockLocation
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var location = await context.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (location == null)
                return StockLocationErrors.NotFound(command.Id);

            location.Restore();

            context.Set<StockLocation>().Update(location);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
