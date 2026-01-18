using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Inventories.Services.Fulfillment;

/// <summary>
/// Default implementation of the fulfillment planner using a "Greedy Minimization Strategy."
/// Prioritizes the 'Default' warehouse to minimize shipping complexity and costs.
/// </summary>
public class DefaultFulfillmentPlanner(IApplicationDbContext context) : IFulfillmentPlanner
{
    public async Task<ErrorOr<FulfillmentPlan>> PlanFulfillmentAsync(
        IDictionary<Guid, int> requestedItems, 
        Guid? customerAddressId = null, 
        CancellationToken ct = default)
    {
        // Guard: Prevent planning for noise
        if (requestedItems == null || !requestedItems.Any())
            return FulfillmentErrors.EmptyOrder;

        // --- STEP 1: Network Discovery ---
        // Load all active, fulfillable warehouses. Prioritizing the 'Default' one.
        // Rule: Only Warehouse/Retail types are considered 'IsFulfillable'.
        var locations = await context.Set<StockLocation>()
            .Where(l => !l.IsDeleted && l.Active)
            .OrderByDescending(l => l.IsDefault) 
            .ToListAsync(ct);

        if (!locations.Any())
            return FulfillmentErrors.NoFulfillableLocations;

        // --- STEP 2: Bulk SKU Mapping (Performance at Scale) ---
        // For 4,000+ products, we perform a single set-based query to get all relevant stock levels.
        var variantIds = requestedItems.Keys.ToList();
        var stockItems = await context.Set<StockItem>()
            .Where(s => variantIds.Contains(s.VariantId) && !s.IsDeleted)
            .ToListAsync(ct);

        var plan = new FulfillmentPlan();
        var remainingToAllocate = new Dictionary<Guid, int>(requestedItems);
        var shipmentDict = new Dictionary<Guid, FulfillmentShipment>();

        // --- STEP 3: Greedy Allocation Loop ---
        // Iterate through warehouses in priority order. Try to empty the shelf of the current 
        // warehouse before moving to the next one to minimize split shipments.
        foreach (var location in locations)
        {
            if (!location.IsFulfillable) continue;

            foreach (var variantId in remainingToAllocate.Keys.ToList())
            {
                var needed = remainingToAllocate[variantId];
                if (needed <= 0) continue;

                // Find the specific balance for this SKU at this location
                var stock = stockItems.FirstOrDefault(s => s.StockLocationId == location.Id && s.VariantId == variantId);
                if (stock == null || stock.CountAvailable <= 0) continue;

                // Calculation: How much physical stock can we 'claim' from this shelf?
                var available = stock.CountAvailable;
                var toTake = Math.Min(needed, available);

                // Proposed Shipment Creation
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

                // Update tracking: subtract allocated from total needed
                remainingToAllocate[variantId] -= toTake;
            }
        }

        // --- STEP 4: Backorder Resolution (Safety Net) ---
        // If items are still needed after checking every warehouse, they are flagged as Backordered.
        // Logic: Debt is logically attached to the 'Default' location for management.
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

            // Find SKU for backorder reference (Fallback to VAR-ID if SKU unknown)
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