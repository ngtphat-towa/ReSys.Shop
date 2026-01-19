using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Admin.Inventories.Transfers.ShipStockTransfer;

public static class ShipStockTransfer
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var transfer = await context.Set<StockTransfer>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (transfer == null)
                return StockTransferErrors.NotFound(command.Id);

            // 1. Mark as shipped in Domain (Workflow Guard)
            var shipResult = transfer.Ship();
            if (shipResult.IsError) return shipResult.Errors;

            // 2. Physical Deduction: Remove stock from Source Location
            foreach (var item in transfer.Items)
            {
                var stockItem = await context.Set<StockItem>()
                    .FirstOrDefaultAsync(x => x.VariantId == item.VariantId && x.StockLocationId == transfer.SourceLocationId, ct);

                if (stockItem == null)
                    return Error.Conflict("StockTransfer.NoSourceStock", $"No stock record found for Variant {item.VariantId} at source location.");

                var adjustResult = stockItem.AdjustStock(
                    -item.Quantity, 
                    StockMovementType.Transfer, 
                    0, // In internal transfers, cost usually stays same
                    $"Transfer Out: {transfer.ReferenceNumber}", 
                    transfer.Id.ToString());

                if (adjustResult.IsError) return adjustResult.Errors;
                
                context.Set<StockItem>().Update(stockItem);
            }

            context.Set<StockTransfer>().Update(transfer);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
