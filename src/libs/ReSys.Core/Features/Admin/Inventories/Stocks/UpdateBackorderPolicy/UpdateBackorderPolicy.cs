using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Features.Admin.Inventories.Stocks.Common;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.UpdateBackorderPolicy;

public static class UpdateBackorderPolicy
{
    public record Command(Guid Id, BackorderPolicyRequest Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new BackorderPolicyValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var stockItem = await context.Set<StockItem>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (stockItem == null)
                return StockItemErrors.NotFound(command.Id);

            var result = stockItem.SetBackorderPolicy(command.Request.Backorderable, command.Request.BackorderLimit);
            if (result.IsError) return result.Errors;

            context.Set<StockItem>().Update(stockItem);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
