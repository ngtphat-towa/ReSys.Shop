using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering.Shipments;
using ReSys.Core.Domain.Ordering.InventoryUnits;
using ReSys.Core.Features.Ordering.Common;
using Mapster;

namespace ReSys.Core.Features.Ordering.Shipments.MarkShipmentPicked;

public static class MarkShipmentPicked
{
    public record Command(
        Guid ShipmentId,
        IDictionary<Guid, string>? UnitSerialNumbers = null) : IRequest<ErrorOr<ShipmentResponse>>;

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
            // 1. Fetch Shipment with its units
            var shipment = await context.Set<Shipment>()
                .Include(x => x.InventoryUnits)
                .FirstOrDefaultAsync(x => x.Id == command.ShipmentId, ct);

            if (shipment == null) return ShipmentErrors.NotFound(command.ShipmentId);

            // 2. Domain Action: Transition state
            var result = shipment.MarkAsPicked();
            if (result.IsError) return result.Errors;

            // 3. Optional: Assign serial numbers during pick (Scanning)
            if (command.UnitSerialNumbers != null)
            {
                foreach (var unit in shipment.InventoryUnits)
                {
                    if (command.UnitSerialNumbers.TryGetValue(unit.Id, out var serial))
                    {
                        unit.SetSerialNumber(serial);
                    }
                }
            }

            // 4. Persistence
            context.Set<Shipment>().Update(shipment);
            await context.SaveChangesAsync(ct);

            return shipment.Adapt<ShipmentResponse>();
        }
    }
}
