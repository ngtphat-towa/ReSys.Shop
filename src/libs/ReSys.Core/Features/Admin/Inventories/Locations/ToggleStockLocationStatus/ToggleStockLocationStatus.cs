using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Admin.Inventories.Locations.ToggleStockLocationStatus;

public static class ToggleStockLocationStatus
{
    public record Command(Guid Id, bool Activate) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var location = await context.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (location == null)
                return StockLocationErrors.NotFound(command.Id);

            if (command.Activate)
            {
                location.Activate();
            }
            else
            {
                var result = location.Deactivate();
                if (result.IsError) return result.Errors;
            }

            context.Set<StockLocation>().Update(location);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
