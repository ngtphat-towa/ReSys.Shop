using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;

/// <summary>
/// Infrastructure service responsible for maintaining the eventual consistency 
/// of the commercial StockSummary projection.
/// </summary>
public interface IInventoryProjectionService
{
    Task RebuildSummaryAsync(Guid variantId, CancellationToken ct = default);
}

public class InventoryProjectionService(IApplicationDbContext context) : IInventoryProjectionService
{
    public async Task RebuildSummaryAsync(Guid variantId, CancellationToken ct = default)
    {
        // Business Rule: Projection only counts stock from Fulfillable (Active/Warehouse) locations.
        var stockData = await context.Set<StockItem>()
            .Include(x => x.StockLocation)
            .Where(x => x.VariantId == variantId && !x.IsDeleted && x.StockLocation.Active)
            .Where(x => x.StockLocation.Type == StockLocationType.Warehouse || x.StockLocation.Type == StockLocationType.RetailStore)
            .ToListAsync(ct);

        if (!stockData.Any()) return;

        // Sum across the network
        var totalOnHand = stockData.Sum(x => x.QuantityOnHand);
        var totalReserved = stockData.Sum(x => x.QuantityReserved);
        var backorderable = stockData.Any(x => x.Backorderable);

        var summary = await context.Set<StockSummary>()
            .FirstOrDefaultAsync(x => x.VariantId == variantId, ct);

        if (summary == null)
        {
            summary = StockSummary.Create(variantId, totalOnHand, totalReserved, backorderable);
            context.Set<StockSummary>().Add(summary);
        }
        else
        {
            summary.Update(totalOnHand, totalReserved, backorderable);
            context.Set<StockSummary>().Update(summary);
        }

        await context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Event Handler that triggers the projection rebuild whenever the ledger changes.
/// </summary>
public class InventoryProjectionHandler(IInventoryProjectionService projectionService) :
    INotificationHandler<StockItemEvents.StockItemCreated>,
    INotificationHandler<StockItemEvents.StockAdjusted>,
    INotificationHandler<StockItemEvents.StockReserved>,
    INotificationHandler<StockItemEvents.StockReleased>,
    INotificationHandler<StockItemEvents.StockFilled>,
    INotificationHandler<StockItemEvents.BackorderPolicyChanged>
{
    public Task Handle(StockItemEvents.StockItemCreated n, CancellationToken ct) => projectionService.RebuildSummaryAsync(n.StockItem.VariantId, ct);
    public Task Handle(StockItemEvents.StockAdjusted n, CancellationToken ct) => projectionService.RebuildSummaryAsync(n.StockItem.VariantId, ct);
    public Task Handle(StockItemEvents.StockReserved n, CancellationToken ct) => projectionService.RebuildSummaryAsync(n.StockItem.VariantId, ct);
    public Task Handle(StockItemEvents.StockReleased n, CancellationToken ct) => projectionService.RebuildSummaryAsync(n.StockItem.VariantId, ct);
    public Task Handle(StockItemEvents.StockFilled n, CancellationToken ct) => projectionService.RebuildSummaryAsync(n.StockItem.VariantId, ct);
    public Task Handle(StockItemEvents.BackorderPolicyChanged n, CancellationToken ct) => projectionService.RebuildSummaryAsync(n.StockItem.VariantId, ct);
}
