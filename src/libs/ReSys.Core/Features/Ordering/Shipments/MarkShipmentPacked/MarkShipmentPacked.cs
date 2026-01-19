using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Shipments.MarkShipmentPacked;

public static class MarkShipmentPacked
{
    public record Command(Guid ShipmentId) : IRequest<ErrorOr<ShipmentResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ShipmentId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<ShipmentResponse>>
    {
        public async Task<ErrorOr<ShipmentResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch
            var shipment = await context.Set<Shipment>()
                .FirstOrDefaultAsync(x => x.Id == command.ShipmentId, ct);

            if (shipment == null) return ShipmentErrors.NotFound(command.ShipmentId);

            // 2. Domain Action
            var result = shipment.MarkAsPacked();
            if (result.IsError) return result.Errors;

            // 3. Persistence
            context.Set<Shipment>().Update(shipment);
            await context.SaveChangesAsync(ct);

            return shipment.Adapt<ShipmentResponse>();
        }
    }
}
