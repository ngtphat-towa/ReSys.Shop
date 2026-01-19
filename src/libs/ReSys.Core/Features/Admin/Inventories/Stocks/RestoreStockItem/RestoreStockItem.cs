using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.RestoreStockItem;

public static class RestoreStockItem
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var stockItem = await context.Set<StockItem>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (stockItem == null)
                return StockItemErrors.NotFound(command.Id);

            var result = stockItem.Restore();
            if (result.IsError) return result.Errors;

            context.Set<StockItem>().Update(stockItem);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
