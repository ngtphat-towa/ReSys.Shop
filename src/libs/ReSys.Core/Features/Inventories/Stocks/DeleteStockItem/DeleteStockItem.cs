using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Inventories.Stocks.DeleteStockItem;

public static class DeleteStockItem
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var stockItem = await context.Set<StockItem>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (stockItem == null)
                return StockItemErrors.NotFound(command.Id);

            var result = stockItem.Delete();
            if (result.IsError) return result.Errors;

            context.Set<StockItem>().Update(stockItem);
            await context.SaveChangesAsync(ct);

            return Result.Deleted;
        }
    }
}
