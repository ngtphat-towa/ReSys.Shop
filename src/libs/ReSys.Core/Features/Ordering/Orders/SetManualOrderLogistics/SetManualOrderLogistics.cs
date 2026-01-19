using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Settings.ShippingMethods;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Orders.SetManualOrderLogistics;

public static class SetManualOrderLogistics
{
    public record Command(
        Guid OrderId,
        Guid ShippingMethodId,
        long? OverrideCostCents = null) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.ShippingMethodId).NotEmpty();
            RuleFor(x => x.OverrideCostCents).GreaterThanOrEqualTo(0).When(x => x.OverrideCostCents.HasValue);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch Order with full financial graph
            var order = await context.Set<Order>()
                .Include(x => x.LineItems).ThenInclude(li => li.Adjustments)
                .Include(x => x.OrderAdjustments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.OrderId, ct);

            if (order == null) return OrderErrors.NotFound(command.OrderId);

            // 2. Verify Shipping Method exists
            var methodExists = await context.Set<ShippingMethod>().AnyAsync(x => x.Id == command.ShippingMethodId, ct);
            if (!methodExists) return Error.NotFound("Ordering.ShippingMethodNotFound", "The selected shipping method does not exist.");

            // 3. Domain Action: Set method and optional override
            var result = order.SetShippingMethod(command.ShippingMethodId, command.OverrideCostCents);
            if (result.IsError) return result.Errors;

            // 4. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}
