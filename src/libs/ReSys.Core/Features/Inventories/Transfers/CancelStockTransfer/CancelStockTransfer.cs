using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Inventories.Transfers.CancelStockTransfer;

public static class CancelStockTransfer
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

            var oldStatus = transfer.Status;

            // 1. Mark as canceled in Domain (Workflow Guard)
            var cancelResult = transfer.Cancel();
            if (cancelResult.IsError) return cancelResult.Errors;

            // 2. Physical Reversal: If items were already "shipped" from source, put them back
            if (oldStatus == StockTransferStatus.InTransit)
            {
                foreach (var item in transfer.Items)
                {
                    var stockItem = await context.Set<StockItem>()
                        .FirstOrDefaultAsync(x => x.VariantId == item.VariantId && x.StockLocationId == transfer.SourceLocationId, ct);

                    if (stockItem != null)
                    {
                        var adjustResult = stockItem.AdjustStock(
                            item.Quantity, 
                            StockMovementType.Correction, 
                            0, 
                            $"Transfer Canceled: {transfer.ReferenceNumber}", 
                            transfer.Id.ToString());

                        if (adjustResult.IsError) return adjustResult.Errors;
                        
                        context.Set<StockItem>().Update(stockItem);
                    }
                }
            }

            context.Set<StockTransfer>().Update(transfer);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
