using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Inventories.Units.Common;

namespace ReSys.Core.Features.Inventories.Units.UpdateInventoryUnitSerialNumber;

public static class UpdateInventoryUnitSerialNumber
{
    public record Command(Guid Id, UpdateSerialNumberRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request.SerialNumber).NotEmpty().MaximumLength(InventoryUnitConstraints.SerialNumberMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var unit = await context.Set<InventoryUnit>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (unit == null)
                return InventoryUnitErrors.NotFound(command.Id);

            // Check for serial number uniqueness
            if (await context.Set<InventoryUnit>().AnyAsync(x => x.SerialNumber == command.Request.SerialNumber && x.Id != unit.Id, ct))
                return Error.Conflict("InventoryUnit.DuplicateSerialNumber", "Serial number already assigned to another unit.");

            unit.SetSerialNumber(command.Request.SerialNumber);

            context.Set<InventoryUnit>().Update(unit);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
