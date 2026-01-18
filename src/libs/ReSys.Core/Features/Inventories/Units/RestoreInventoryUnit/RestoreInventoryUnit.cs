using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Inventories.Units.RestoreInventoryUnit;

public static class RestoreInventoryUnit
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var unit = await context.Set<InventoryUnit>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (unit == null)
                return InventoryUnitErrors.NotFound(command.Id);

            var result = unit.Restore();
            if (result.IsError) return result.Errors;

            context.Set<InventoryUnit>().Update(unit);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
