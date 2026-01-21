using MediatR;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Domain.Ordering.Payments.Gateways;

using Microsoft.EntityFrameworkCore;

namespace ReSys.Core.Features.Admin.Ordering.Payments.Handlers;

/// <summary>
/// Automatically triggers refunds when an order is canceled.
/// </summary>
public sealed class OrderCanceledRefundHandler(
    IApplicationDbContext dbContext,
    IPaymentProcessorFactory paymentFactory) : INotificationHandler<OrderEvents.OrderCanceled>
{
    public async Task Handle(OrderEvents.OrderCanceled notification, CancellationToken ct)
    {
        var order = notification.Order;

        // Ensure Payments and Config are loaded
        var paymentsToRefund = await dbContext.Set<Payment>()
            .Include(p => p.PaymentMethod)
            .Where(p => p.OrderId == order.Id && p.State == Payment.PaymentState.Completed)
            .ToListAsync(ct);

        foreach (var payment in paymentsToRefund)
        {
            if (payment.PaymentMethod == null) continue;

            var processorResult = paymentFactory.GetProcessor(payment.PaymentMethod.Type);
            if (processorResult.IsError) continue;

            // Execute External Refund
            var result = await processorResult.Value.RefundAsync(
                payment.PaymentMethod,
                payment,
                payment.AmountCents, // Full refund
                ct);

            // Update Local State
            if (!result.IsError)
            {
                payment.Refund(payment.AmountCents, "Order Canceled");
            }
        }

        if (paymentsToRefund.Any())
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
