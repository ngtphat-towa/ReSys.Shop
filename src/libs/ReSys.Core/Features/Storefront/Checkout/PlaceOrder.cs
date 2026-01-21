using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Domain.Ordering.Payments.Gateways;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Core.Domain.Inventories.Services;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Features.Storefront.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Storefront.Checkout;

public static class PlaceOrder
{
    public record Command(PlaceOrderRequest Request) : IRequest<ErrorOr<OrderPlacementResult>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.PaymentMethodId).NotEmpty();
        }
    }

    public class Handler(
        IApplicationDbContext dbContext, 
        IUserContext userContext,
        IPaymentProcessorFactory paymentFactory,
        IInventoryReservationService inventoryService) : IRequestHandler<Command, ErrorOr<OrderPlacementResult>>
    {
        public async Task<ErrorOr<OrderPlacementResult>> Handle(Command command, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return Error.Unauthorized("Checkout.LoginRequired", "Please log in to complete your purchase.");

            var userId = userContext.UserId;

            var order = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .Include(o => o.ShipAddress)
                .Include(o => o.BillAddress)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.UserId == userId && (o.State == Order.OrderState.Delivery || o.State == Order.OrderState.Payment), ct);

            if (order == null) return Error.NotFound("Order.NotFound", "No active checkout found for this user.");

            if (order.State == Order.OrderState.Delivery)
            {
                var transition = order.Next(); // Delivery -> Payment
                if (transition.IsError) return transition.Errors;
            }

            var paymentMethod = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == command.Request.PaymentMethodId && pm.Status == PaymentMethod.PaymentStatus.Active, ct);

            if (paymentMethod == null) return Error.NotFound("PaymentMethod.NotFound");

            // 2. Inventory Reservation (Idempotent)
            // Guard: Prevent double-reservation if the user retries the request.
            var hasReservations = order.LineItems.Any(li => li.InventoryUnits.Any(u => u.State != InventoryUnitState.Canceled));
            
            if (!hasReservations)
            {
                var reservationResult = await inventoryService.AttemptReservationAsync(order.Id, order.LineItems, ct);
                if (reservationResult.IsError) return reservationResult.Errors;

                var units = await dbContext.Set<InventoryUnit>()
                    .Where(u => u.OrderId == order.Id)
                    .ToListAsync(ct);

                var groupedUnits = units.GroupBy(u => u.StockLocationId ?? Guid.Empty);
                foreach (var group in groupedUnits)
                {
                    if (group.Key == Guid.Empty) continue;

                    var shipmentResult = Shipment.Create(order.Id, group.Key, 0);
                    if (shipmentResult.IsError) 
                    {
                        await inventoryService.ReleaseReservationAsync(order.Id, ct);
                        return shipmentResult.Errors;
                    }
                    
                    var shipment = shipmentResult.Value;
                    dbContext.Set<Shipment>().Add(shipment);

                    foreach (var unit in group)
                    {
                        unit.AssignToShipment(shipment.Id);
                    }
                }
                await dbContext.SaveChangesAsync(ct);
            }

            // 3. Payment Creation (Idempotent)
            // Guard: Void any previous pending payments to prevent multiple active intents for the same order.
            var pendingPayments = order.Payments.Where(p => p.State == Payment.PaymentState.Pending).ToList();
            foreach (var previousPayment in pendingPayments)
            {
                previousPayment.Void();
            }

            var paymentResult = Payment.Create(
                orderId: order.Id,
                amountCents: (long)order.TotalCents, // Cast to long if entity uses decimal, but our Payment.Create takes long
                currency: order.Currency,
                paymentMethodType: paymentMethod.Type.ToString(),
                paymentMethodId: paymentMethod.Id);

            if (paymentResult.IsError) 
            {
                await inventoryService.ReleaseReservationAsync(order.Id, ct);
                return paymentResult.Errors;
            }
            var payment = paymentResult.Value;

            var addResult = order.AddPayment(payment);
            if (addResult.IsError)
            {
                await inventoryService.ReleaseReservationAsync(order.Id, ct);
                return addResult.Errors;
            }

            var processorResult = paymentFactory.GetProcessor(paymentMethod.Type);
            if (processorResult.IsError)
            {
                await inventoryService.ReleaseReservationAsync(order.Id, ct);
                return processorResult.Errors;
            }

            var processResult = await processorResult.Value.ProcessPaymentAsync(
                paymentMethod,
                payment,
                (long)order.TotalCents,
                order.Currency,
                ct);

            if (processResult.IsError)
            {
                await inventoryService.ReleaseReservationAsync(order.Id, ct);
                return processResult.Errors;
            }

            payment.MarkAsAuthorized(processResult.Value.TransactionId);

            await dbContext.SaveChangesAsync(ct);

            return new OrderPlacementResult(processResult.Value.ClientSecret ?? "", processResult.Value.TransactionId);
        }
    }
}