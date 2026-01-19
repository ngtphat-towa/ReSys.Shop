using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Inventories.Units.MarkInventoryUnitDamaged;

public static class MarkInventoryUnitDamaged
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var unit = await context.Set<InventoryUnit>()
                .Include(x => x.StockItem)
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (unit == null)
                return InventoryUnitErrors.NotFound(command.Id);

            // 1. Domain Action: Mark as damaged
            unit.MarkAsDamaged();

            if (unit.StockItem != null)
            {
                // 2. Physical Deduction: This item is no longer sellable physical stock
                // We must reduce QuantityOnHand and record a Loss movement.
                var result = unit.StockItem.AdjustStock(
                    -1, 
                    StockMovementType.Loss, 
                    0, 
                    $"Unit {unit.Id} marked as damaged", 
                    unit.Id.ToString());

                if (result.IsError) return result.Errors;
                context.Set<StockItem>().Update(unit.StockItem);
            }

            context.Set<InventoryUnit>().Update(unit);
            
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
