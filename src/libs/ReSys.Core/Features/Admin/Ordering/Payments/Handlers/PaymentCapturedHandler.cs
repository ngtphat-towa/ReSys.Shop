using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.Payments;

namespace ReSys.Core.Features.Admin.Ordering.Payments.Handlers;

/// <summary>
/// Reacts to external payment capture events (e.g. Stripe Webhooks)
/// to update the local order state.
/// </summary>
public sealed class PaymentCapturedHandler(IApplicationDbContext dbContext)
    : INotificationHandler<PaymentEvents.ExternalPaymentCaptured>
{
    public async Task Handle(PaymentEvents.ExternalPaymentCaptured notification, CancellationToken ct)
    {
        // 1. Load Aggregate Root
        // We include Payments to ensure we can find the child entity
        var order = await dbContext.Set<Order>()
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, ct);

        if (order == null)
        {
            // Logging is handled by the behavior, but we might want to log specific business warning here
            return;
        }

        // 2. Domain Logic
        // Use the explicit domain method we added
        var captureResult = order.CapturePayment(notification.PaymentId, notification.TransactionId);

        if (captureResult.IsError)
        {
            // If the payment is not found or already captured, we log it.
            // Ideally we might raise a "PaymentCaptureFailed" event or similar.
            return;
        }

        // 3. Operational Logic: Auto-Completion
        // If the order is fully paid, we push it through the final stages (Payment -> Confirm -> Complete).
        // This ensures the warehouse system receives the order immediately.
        var totalPaid = order.Payments
            .Where(p => p.State == Payment.PaymentState.Completed)
            .Sum(p => p.AmountCents);

        if (totalPaid >= order.TotalCents)
        {
            // Transition: Payment -> Confirm
            if (order.State == Order.OrderState.Payment)
            {
                var confirmResult = order.Next();
                if (confirmResult.IsError)
                {
                    // Log error but persist the payment capture
                    return;
                }
            }

            // Transition: Confirm -> Complete
            if (order.State == Order.OrderState.Confirm)
            {
                var completeResult = order.Next();
                if (completeResult.IsError)
                {
                    // Log error (e.g. inventory allocation failed at last second)
                    return;
                }
            }
        }

        // 4. Persistence
        await dbContext.SaveChangesAsync(ct);
    }
}
