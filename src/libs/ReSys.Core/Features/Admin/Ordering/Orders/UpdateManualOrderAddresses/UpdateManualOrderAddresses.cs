using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Features.Admin.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Admin.Ordering.Orders.UpdateManualOrderAddresses;

public static class UpdateManualOrderAddresses
{
    public record Command(
        Guid OrderId,
        Guid ShippingAddressId,
        Guid BillingAddressId) : IRequest<ErrorOr<OrderDetailResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.ShippingAddressId).NotEmpty();
            RuleFor(x => x.BillingAddressId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<OrderDetailResponse>>
    {
        public async Task<ErrorOr<OrderDetailResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch Order with history
            var order = await context.Set<Order>()
                .Include(x => x.Histories)
                .FirstOrDefaultAsync(x => x.Id == command.OrderId, ct);

            if (order == null) return OrderErrors.NotFound(command.OrderId);

            // 2. Fetch Addresses
            var shipping = await context.Set<UserAddress>()
                .FirstOrDefaultAsync(x => x.Id == command.ShippingAddressId, ct);
            
            var billing = await context.Set<UserAddress>()
                .FirstOrDefaultAsync(x => x.Id == command.BillingAddressId, ct);

            if (shipping == null || billing == null)
                return Error.NotFound("Ordering.AddressNotFound", "One or more addresses were not found.");

            // 3. Domain Action
            var result = order.SetAddresses(shipping, billing);
            if (result.IsError) return result.Errors;

            // 4. Persistence
            context.Set<Order>().Update(order);
            await context.SaveChangesAsync(ct);

            return order.Adapt<OrderDetailResponse>();
        }
    }
}
