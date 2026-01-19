using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Admin.Inventories.Services.Fulfillment;
using ReSys.Core.Features.Admin.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Admin.Ordering.Fulfillment.FinalizeFulfillment;

public static class FinalizeFulfillment
{
    public record Command(Guid OrderId, FulfillmentPlan Plan) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.Plan).NotNull();
            RuleFor(x => x.Plan.Shipments).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch the Order with its granular unit placeholders
            var order = await context.Set<Order>()
                .Include(x => x.LineItems).ThenInclude(li => li.InventoryUnits)
                .Include(x => x.Shipments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.OrderId, ct);

            if (order == null) return OrderErrors.NotFound(command.OrderId);

            // 2. Clear any existing draft shipments if re-running fulfillment
            if (order.State < Order.OrderState.Payment)
            {
                order.Shipments.Clear();
            }

            // 3. Process each shipment in the plan
            foreach (var proposedShipment in command.Plan.Shipments)
            {
                var shipmentResult = Shipment.Create(order.Id, proposedShipment.StockLocationId);
                if (shipmentResult.IsError) return shipmentResult.Errors;
                
                var shipment = shipmentResult.Value;
                order.AddShipment(shipment);

                // 4. Assign specific physical units
                foreach (var proposedItem in proposedShipment.Items)
                {
                    // Find available placeholders for this variant
                    var placeholders = order.LineItems
                        .First(li => li.VariantId == proposedItem.VariantId)
                        .InventoryUnits
                        .Where(u => u.ShipmentId == null)
                        .Take(proposedItem.Quantity)
                        .ToList();

                    foreach (var unit in placeholders)
                    {
                        if (proposedItem.IsBackordered)
                        {
                            unit.Backorder(order.Id);
                        }
                        else 
                        {
                            // ERP Rule: Lock the specific StockItem for this unit
                            var stockItem = await context.Set<StockItem>()
                                .FirstAsync(s => s.StockLocationId == proposedShipment.StockLocationId && s.VariantId == unit.VariantId, ct);
                            
                            unit.SetStockItem(stockItem.Id, proposedShipment.StockLocationId);
                            unit.Reserve(order.Id);
                        }

                        unit.AssignToShipment(shipment.Id);
                    }
                }
            }

            // 5. Audit the allocation decision
            order.SetMetadata(
                new Dictionary<string, object?>(order.PublicMetadata), 
                new Dictionary<string, object?>(order.PrivateMetadata) 
                { 
                    { "FulfillmentSource", "AdminProposal" },
                    { "AllocatedAt", DateTimeOffset.UtcNow }
                });

            // 6. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}
