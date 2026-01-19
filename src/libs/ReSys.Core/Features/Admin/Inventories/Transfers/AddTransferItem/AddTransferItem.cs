using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Features.Admin.Inventories.Transfers.Common;

namespace ReSys.Core.Features.Admin.Inventories.Transfers.AddTransferItem;

public static class AddTransferItem
{
    public record Command(Guid Id, AddTransferItemRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request.VariantId).NotEmpty();
            RuleFor(x => x.Request.Quantity).GreaterThan(0);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var transfer = await context.Set<StockTransfer>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (transfer == null)
                return StockTransferErrors.NotFound(command.Id);

            var result = transfer.AddItem(command.Request.VariantId, command.Request.Quantity);
            if (result.IsError) return result.Errors;

            context.Set<StockTransfer>().Update(transfer);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
