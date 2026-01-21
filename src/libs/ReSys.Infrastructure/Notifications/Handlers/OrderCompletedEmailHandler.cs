using MediatR;
using ReSys.Core.Common.Notifications.Constants;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Common.Notifications.Models;
using ReSys.Core.Domain.Ordering;

namespace ReSys.Infrastructure.Notifications.Handlers;

/// <summary>
/// Listens for completed orders and sends a confirmation email to the customer.
/// Uses the structured Notification system with defined UseCases and Parameters.
/// </summary>
public sealed class OrderCompletedEmailHandler(INotificationService notificationService) 
    : INotificationHandler<OrderEvents.OrderCompleted>
{
    public async Task Handle(OrderEvents.OrderCompleted notification, CancellationToken ct)
    {
        var order = notification.Order;
        
        // Guard: Email required
        if (string.IsNullOrEmpty(order.Email)) return;

        // Build Context
        var context = NotificationContext.Create(
            (NotificationConstants.Parameter.OrderId, order.Number),
            (NotificationConstants.Parameter.OrderTotal, (order.TotalCents / 100m).ToString("C")), 
            (NotificationConstants.Parameter.OrderDate, order.CompletedAt?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"))
        );

        var recipient = NotificationRecipient.Create(order.Email, "Customer");

        var message = NotificationMessage.Create(
            NotificationConstants.UseCase.SystemOrderConfirmation,
            recipient,
            context
        );

        // Send notification
        await notificationService.SendAsync(message, ct);
    }
}