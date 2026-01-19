using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Features.Admin.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Admin.Ordering.Shipments.DispatchShipment;

public static class DispatchShipment
{
    public record Command(Guid ShipmentId, string TrackingNumber) : IRequest<ErrorOr<ShipmentResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ShipmentId).NotEmpty();
            RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(ShipmentConstraints.TrackingNumberMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<ShipmentResponse>>
    {
        public async Task<ErrorOr<ShipmentResponse>> Handle(Command command, CancellationToken ct)
        {
            // 1. Fetch with units (Shipment.Ship() updates units too)
            var shipment = await context.Set<Shipment>()
                .Include(x => x.InventoryUnits)
                .FirstOrDefaultAsync(x => x.Id == command.ShipmentId, ct);

            if (shipment == null) return ShipmentErrors.NotFound(command.ShipmentId);

            // 2. Domain Action: Final handoff to carrier
            var result = shipment.Ship(command.TrackingNumber);
            if (result.IsError) return result.Errors;

            // 3. Persistence
            context.Set<Shipment>().Update(shipment);
            await context.SaveChangesAsync(ct);

            return shipment.Adapt<ShipmentResponse>();
        }
    }
}
