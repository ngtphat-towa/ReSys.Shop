using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Admin.Inventories.Stocks.Common;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.AdjustStock;

public static class AdjustStock
{
    public record Command(Guid Id, StockAdjustmentRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new StockAdjustmentValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var request = command.Request;

            // 1. Load: the Aggregate Root with necessary history
            var stockItem = await context.Set<StockItem>()
                .Include(x => x.InventoryUnits) // Needed for backorder promotion
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (stockItem == null)
                return StockItemErrors.NotFound(command.Id);

            // 2. Act: Domain adjustment
            var result = stockItem.AdjustStock(
                request.Quantity, 
                request.Type, 
                request.UnitCost, 
                request.Reason, 
                request.Reference);

            if (result.IsError) return result.Errors;

            // 3. Persist: Explicitly update context
            context.Set<StockItem>().Update(stockItem);
            
            // Explicitly add new movements created during the adjustment
            foreach (var movement in stockItem.StockMovements.Where(m => m.CreatedAt >= DateTimeOffset.UtcNow.AddSeconds(-5)))
            {
                context.Set<Domain.Inventories.Movements.StockMovement>().Add(movement);
            }

            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
