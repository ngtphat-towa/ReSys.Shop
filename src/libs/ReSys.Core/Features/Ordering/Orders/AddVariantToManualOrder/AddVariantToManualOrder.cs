using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Orders.AddVariantToManualOrder;

public static class AddVariantToManualOrder
{
    public record Command(
        Guid OrderId,
        Guid VariantId,
        int Quantity,
        long? OverridePriceCents = null) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.VariantId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(LineItemConstraints.MinQuantity);
            RuleFor(x => x.OverridePriceCents).GreaterThanOrEqualTo(0).When(x => x.OverridePriceCents.HasValue);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch the Order with its full graph (Essential for RecalculateTotals)
            var order = await context.Set<Order>()
                .Include(x => x.LineItems).ThenInclude(li => li.Adjustments)
                .Include(x => x.OrderAdjustments)
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.OrderId, ct);

            if (order == null) return OrderErrors.NotFound(command.OrderId);

            // 2. Fetch the Variant with its Product metadata for the Snapshot
            var variant = await context.Set<Variant>()
                .Include(v => v.Product)
                .Include(v => v.OptionValues)
                .FirstOrDefaultAsync(v => v.Id == command.VariantId, ct);

            if (variant == null) return VariantErrors.NotFound(command.VariantId);

            // 3. Domain Action: Add variant with Snapshot & Override logic
            var result = order.AddVariant(variant, command.Quantity, DateTimeOffset.UtcNow, command.OverridePriceCents);
            if (result.IsError) return result.Errors;

            // 4. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            // 5. Deep Projection
            return order.Adapt<OrderDetailResponse>();
        }
    }
}
