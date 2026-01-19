using ErrorOr;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Settings.Stores;

namespace ReSys.Core.Features.Inventories.Services.Fulfillment;

/// <summary>
/// Implements a "Greedy Minimization Strategy."
/// Prioritizes the 'Default' warehouse and attempts to satisfy the order from the minimum number of locations
/// linked to the specific store.
/// </summary>
public class GreedyFulfillmentStrategy : IFulfillmentStrategy
{
    public async Task<ErrorOr<FulfillmentPlan>> CreatePlanAsync(
        IApplicationDbContext context,
        Guid storeId,
        IDictionary<Guid, int> requestedItems,
        Guid? customerAddressId = null,
        CancellationToken ct = default)
    {
        // Guard: Prevent planning for noise
        if (requestedItems == null || !requestedItems.Any())
            return FulfillmentErrors.EmptyOrder;

        // --- STEP 1: Network Discovery (Store-Aware) ---
        // Fetch only StockLocations that are linked to this store and active
        var storeLocations = await context.Set<StoreStockLocation>()
            .Include(ssl => ssl.StockLocation)
            .Where(ssl => ssl.StoreId == storeId && ssl.IsActive && !ssl.StockLocation.IsDeleted && ssl.StockLocation.Active)
            .OrderByDescending(ssl => ssl.StockLocation.IsDefault)
            .ThenBy(ssl => ssl.Priority)
            .ToListAsync(ct);

        if (!storeLocations.Any())
            return FulfillmentErrors.NoFulfillableLocations;

        var locations = storeLocations
            .Select(ssl => ssl.StockLocation)
            .Where(l => l.IsFulfillable)
            .ToList();

        if (!locations.Any())
            return FulfillmentErrors.NoFulfillableLocations;

        // --- STEP 2: Bulk SKU Mapping ---
        var variantIds = requestedItems.Keys.ToList();
        var locationIds = locations.Select(l => l.Id).ToList();
        
        var stockItems = await context.Set<StockItem>()
            .Where(s => variantIds.Contains(s.VariantId) && locationIds.Contains(s.StockLocationId) && !s.IsDeleted)
            .ToListAsync(ct);

        var plan = new FulfillmentPlan();
        var remainingToAllocate = new Dictionary<Guid, int>(requestedItems);
        var shipmentDict = new Dictionary<Guid, FulfillmentShipment>();

        // --- STEP 3: Greedy Allocation Loop ---
        foreach (var location in locations)
        {
            foreach (var variantId in remainingToAllocate.Keys.ToList())
            {
                var needed = remainingToAllocate[variantId];
                if (needed <= 0) continue;

                var stock = stockItems.FirstOrDefault(s => s.StockLocationId == location.Id && s.VariantId == variantId);
                if (stock == null || stock.CountAvailable <= 0) continue;

                var available = stock.CountAvailable;
                var toTake = Math.Min(needed, available);

                if (!shipmentDict.TryGetValue(location.Id, out var shipment))
                {
                    shipment = new FulfillmentShipment
                    {
                        StockLocationId = location.Id,
                        StockLocationName = location.Name
                    };
                    shipmentDict[location.Id] = shipment;
                }

                shipment.Items.Add(new FulfillmentItem
                {
                    VariantId = variantId,
                    Sku = stock.Sku,
                    Quantity = toTake,
                    IsBackordered = false
                });

                remainingToAllocate[variantId] -= toTake;
            }
        }

        // --- STEP 4: Backorder Resolution ---
        // Any remaining items are assigned as backorders to the Store's Default Location
        var defaultLocation = locations.FirstOrDefault(l => l.IsDefault) ?? locations.First();
        foreach (var variantId in remainingToAllocate.Keys.ToList())
        {
            var stillNeeded = remainingToAllocate[variantId];
            if (stillNeeded <= 0) continue;

            if (!shipmentDict.TryGetValue(defaultLocation.Id, out var shipment))
            {
                shipment = new FulfillmentShipment
                {
                    StockLocationId = defaultLocation.Id,
                    StockLocationName = defaultLocation.Name
                };
                shipmentDict[defaultLocation.Id] = shipment;
            }

            var sku = stockItems.FirstOrDefault(s => s.VariantId == variantId)?.Sku ?? $"VAR-{variantId.ToString()[..8]}";

            shipment.Items.Add(new FulfillmentItem
            {
                VariantId = variantId,
                Sku = sku,
                Quantity = stillNeeded,
                IsBackordered = true
            });
        }

        plan.Shipments.AddRange(shipmentDict.Values);
        return plan;
    }
}