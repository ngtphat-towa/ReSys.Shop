using ErrorOr;
using FluentValidation;
using MediatR;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Core.Features.Inventories.Transfers.Common;

namespace ReSys.Core.Features.Inventories.Transfers.CreateStockTransfer;

public static class CreateStockTransfer
{
    public record Request : StockTransferInput;
    public record Response : StockTransferListItem;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.SourceLocationId).NotEmpty();
            RuleFor(x => x.Request.DestinationLocationId).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var request = command.Request;

            var transferResult = StockTransfer.Create(
                request.SourceLocationId,
                request.DestinationLocationId,
                null,
                request.Reason);

            if (transferResult.IsError) return transferResult.Errors;
            var transfer = transferResult.Value;

            context.Set<StockTransfer>().Add(transfer);
            await context.SaveChangesAsync(ct);

            return transfer.Adapt<Response>();
        }
    }
}
