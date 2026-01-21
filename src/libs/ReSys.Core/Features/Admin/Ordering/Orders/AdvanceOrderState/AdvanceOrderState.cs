using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Admin.Ordering.Common;
using Mapster;
using ReSys.Core.Domain.Inventories.Services;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Domain.Ordering.Shipments;

namespace ReSys.Core.Features.Admin.Ordering.Orders.AdvanceOrderState;

public static class AdvanceOrderState
{
    public record Command(Guid Id) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Handler(IApplicationDbContext context, IInventoryReservationService inventoryService) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            var order = await context.Set<Order>()
                .Include(x => x.LineItems).ThenInclude(li => li.InventoryUnits)
                .Include(x => x.Payments)
                .Include(x => x.Shipments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (order == null) return OrderErrors.NotFound(command.Id);

            var previousState = order.State;

            // 2. Domain Action: Transition to next logical state
            var result = order.Next();
            if (result.IsError) return result.Errors;

            // 3. Operational Logic: Inventory Reservation & Fulfillment Trigger
            // Triggered when moving from Delivery -> Payment (Commitment point)
            if (previousState == Order.OrderState.Delivery && order.State == Order.OrderState.Payment)
            {
                // 3a. Reserve Inventory
                var reservation = await inventoryService.AttemptReservationAsync(order.Id, order.LineItems, ct);
                if (reservation.IsError) return reservation.Errors; // Validation fail, DB changes discarded

                // 3b. Auto-create Shipments
                // Note: InventoryUnits are created by Reservation but might not be attached to LineItems in memory yet depending on tracking.
                // We fetch them explicitly to be safe.
                var units = await context.Set<InventoryUnit>()
                    .Where(u => u.OrderId == order.Id)
                    .ToListAsync(ct);

                var grouped = units.GroupBy(u => u.StockLocationId ?? Guid.Empty);
                foreach (var group in grouped)
                {
                    if (group.Key == Guid.Empty) continue;

                    var shipmentResult = Shipment.Create(order.Id, group.Key, 0); // Cost logic handled in Order
                    if (shipmentResult.IsError) return shipmentResult.Errors;

                    var shipment = shipmentResult.Value;
                    context.Set<Shipment>().Add(shipment);

                    foreach (var unit in group) unit.AssignToShipment(shipment.Id);
                }
            }

            // 4. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}