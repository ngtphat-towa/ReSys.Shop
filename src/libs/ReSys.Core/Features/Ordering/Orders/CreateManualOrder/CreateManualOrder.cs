using ErrorOr;
using FluentValidation;
using MediatR;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Orders.CreateManualOrder;

public static class CreateManualOrder
{
    public record Command(
        Guid StoreId,
        string Currency,
        string? UserId = null,
        string? Email = null) : IRequest<ErrorOr<OrderSummaryResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.StoreId).NotEmpty();
            RuleFor(x => x.Currency).NotEmpty().Length(3);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderSummaryResponse>>
    {
        public async Task<ErrorOr<OrderSummaryResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Create the Aggregate Root
            var orderResult = Order.Create(
                command.StoreId,
                command.Currency,
                command.UserId,
                command.Email);

            if (orderResult.IsError) return orderResult.Errors;

            var order = orderResult.Value;

            // 2. Persist
            context.Set<Order>().Add(order);
            await context.SaveChangesAsync(ct);

            // 3. Project to Response
            return order.Adapt<OrderSummaryResponse>();
        }
    }
}
