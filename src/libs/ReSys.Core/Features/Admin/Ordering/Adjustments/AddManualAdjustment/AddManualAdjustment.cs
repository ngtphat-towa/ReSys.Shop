using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.Adjustments;
using ReSys.Core.Features.Admin.Ordering.Common;

using Mapster;

namespace ReSys.Core.Features.Admin.Ordering.Adjustments.AddManualAdjustment;

public static class AddManualAdjustment
{
    public record Command(
        Guid OrderId,
        long AmountCents,
        string Description,
        OrderAdjustment.AdjustmentScope Scope = OrderAdjustment.AdjustmentScope.Order) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.Description).NotEmpty().MaximumLength(AdjustmentConstraints.DescriptionMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch Order with financial graph
            var order = await context.Set<Order>()
                .Include(x => x.LineItems).ThenInclude(li => li.Adjustments)
                .Include(x => x.OrderAdjustments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.OrderId, ct);

            if (order == null) return OrderErrors.NotFound(command.OrderId);

            // 2. Domain Action: Create manual adjustment (PromotionId = null)
            var adjResult = OrderAdjustment.Create(
                order.Id,
                command.AmountCents,
                command.Description,
                command.Scope);

            if (adjResult.IsError) return adjResult.Errors;

            // 3. Link and Recalculate
            order.OrderAdjustments.Add(adjResult.Value);
            order.RecalculateTotals();

            // 4. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}
