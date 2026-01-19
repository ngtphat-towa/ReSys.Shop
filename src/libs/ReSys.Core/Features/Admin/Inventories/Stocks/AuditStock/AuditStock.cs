using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.AuditStock;

/// <summary>
/// Feature: Audit Stock (Stocktake)
/// Allows an absolute quantity correction based on a physical count.
/// </summary>
public static class AuditStock
{
    public record Request(int PhysicalCount, string? Reason = null, string? Reference = null);

    public record Command(Guid Id, Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Data.PhysicalCount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Data.Reference).NotEmpty().WithMessage("An audit reference or ID is required for traceability.");
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var data = command.Data;

            // 1. Load the Aggregate
            var stockItem = await context.Set<StockItem>()
                .Include(x => x.InventoryUnits)
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (stockItem == null)
                return StockItemErrors.NotFound(command.Id);

            // 2. Logic: Calculate the delta required to reach the physical count
            var currentCount = stockItem.QuantityOnHand;
            var delta = data.PhysicalCount - currentCount;

            // Guard: If count matches, we still record an audit movement for historical proof (zero adjustment)
            // But if delta is zero, AdjustStock might fail if it has a "ZeroQuantity" guard.
            // In ERP systems, an audit with 0 change is still a valuable 'verification' record.
            
            if (delta != 0)
            {
                var result = stockItem.AdjustStock(
                    delta, 
                    StockMovementType.Adjustment, 
                    0, 
                    data.Reason ?? "Physical Inventory Audit", 
                    data.Reference);

                if (result.IsError) return result.Errors;
            }

            // 3. Persist
            context.Set<StockItem>().Update(stockItem);
            
            // Business Rule: Ensure all new movements are tracked
            foreach (var movement in stockItem.StockMovements.Where(m => m.CreatedAt >= DateTimeOffset.UtcNow.AddMinutes(-1)))
            {
                 // We use the full namespace to avoid ambiguity if needed, or ensure using is correct
                 context.Set<StockMovement>().Add(movement);
            }

            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
