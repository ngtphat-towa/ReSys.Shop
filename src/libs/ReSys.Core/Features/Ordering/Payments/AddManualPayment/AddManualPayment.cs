using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.Payments;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Payments.AddManualPayment;

public static class AddManualPayment
{
    public record Command(
        Guid OrderId,
        long AmountCents,
        string MethodType,
        string? TransactionId = null) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.AmountCents).GreaterThan(0);
            RuleFor(x => x.MethodType).NotEmpty().MaximumLength(PaymentConstraints.PaymentMethodTypeMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch Order
            var order = await context.Set<Order>()
                .Include(x => x.Payments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.OrderId, ct);

            if (order == null) return OrderErrors.NotFound(command.OrderId);

            // 2. Create the Payment in COMPLETED state (Manual override)
            var paymentResult = Payment.Create(
                order.Id,
                command.AmountCents,
                order.Currency,
                command.MethodType);

            if (paymentResult.IsError) return paymentResult.Errors;

            var payment = paymentResult.Value;
            
            // Immediately capture since it's manual (Cash/Wire)
            var captureResult = payment.MarkAsCaptured(command.TransactionId ?? $"MANUAL-{DateTimeOffset.UtcNow.Ticks}");
            if (captureResult.IsError) return captureResult.Errors;

            // 3. Link to Order
            order.Payments.Add(payment);
            
            // 4. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}
