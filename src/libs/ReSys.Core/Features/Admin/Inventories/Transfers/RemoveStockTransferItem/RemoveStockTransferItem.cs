using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Movements;

namespace ReSys.Core.Features.Admin.Inventories.Transfers.RemoveStockTransferItem;

public static class RemoveStockTransferItem
{
    public record Command(Guid Id, Guid VariantId) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var transfer = await context.Set<StockTransfer>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (transfer == null)
                return StockTransferErrors.NotFound(command.Id);

            if (transfer.Status != StockTransferStatus.Draft)
                return StockTransferErrors.InvalidStatusTransition(transfer.Status, "Remove Item");

            var item = transfer.Items.FirstOrDefault(x => x.VariantId == command.VariantId);
            if (item == null)
                return StockTransferErrors.ItemNotFound(command.VariantId);

            transfer.Items.Remove(item);
            context.Set<StockTransfer>().Update(transfer);
            
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
