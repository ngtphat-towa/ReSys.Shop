using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Orders.AdvanceOrderState;

public static class AdvanceOrderState
{
    public record Command(Guid Id) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch Order with full financial and physical graph
            // This is required because Next() validations check payments and allocations.
            var order = await context.Set<Order>()
                .Include(x => x.LineItems).ThenInclude(li => li.InventoryUnits)
                .Include(x => x.Payments)
                .Include(x => x.Shipments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (order == null) return OrderErrors.NotFound(command.Id);

            // 2. Domain Action: Transition to next logical state
            var result = order.Next();
            if (result.IsError) return result.Errors;

            // 3. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}
