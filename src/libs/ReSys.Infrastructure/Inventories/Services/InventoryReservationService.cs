using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Services;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Domain.Ordering.LineItems;

namespace ReSys.Infrastructure.Inventories.Services;

public sealed class InventoryReservationService(IApplicationDbContext dbContext) : IInventoryReservationService
{
    public async Task<ErrorOr<Success>> AttemptReservationAsync(Guid orderId, IEnumerable<LineItem> items, CancellationToken ct)
    {
        var reservedItems = new List<(StockItem Stock, int Quantity)>();

        foreach (var lineItem in items)
        {
            var stockItem = await dbContext.Set<StockItem>()
                .Include(s => s.InventoryUnits)
                .FirstOrDefaultAsync(s => s.VariantId == lineItem.VariantId, ct);

            if (stockItem == null)
            {
                // Rollback in memory (DB transaction handles persistence rollback if we throw, but we want clean ErrorOr)
                await Release(reservedItems, orderId); 
                return Error.NotFound("Stock.NotFound", $"No inventory record found for variant {lineItem.VariantId}");
            }

            var result = stockItem.Reserve(lineItem.Quantity, orderId, lineItem.Id);
            
            if (result.IsError)
            {
                await Release(reservedItems, orderId);
                return result.Errors;
            }

            reservedItems.Add((stockItem, lineItem.Quantity));
        }

        await dbContext.SaveChangesAsync(ct);
        return Result.Success;
    }

    public async Task ReleaseReservationAsync(Guid orderId, CancellationToken ct)
    {
        var stockItems = await dbContext.Set<StockItem>()
            .Include(s => s.InventoryUnits)
            .Where(s => s.InventoryUnits.Any(u => u.OrderId == orderId))
            .ToListAsync(ct);

        foreach (var stock in stockItems)
        {
            var quantity = stock.InventoryUnits.Count(u => u.OrderId == orderId && (u.State == InventoryUnitState.OnHand || u.State == InventoryUnitState.Backordered));
            if (quantity > 0)
            {
                stock.Release(quantity, orderId);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task Release(List<(StockItem Stock, int Quantity)> items, Guid orderId)
    {
        foreach (var (stock, qty) in items)
        {
            stock.Release(qty, orderId);
        }
    }
}
