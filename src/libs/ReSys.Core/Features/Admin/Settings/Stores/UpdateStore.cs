using ErrorOr;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Features.Admin.Settings.Stores.Common;

namespace ReSys.Core.Features.Admin.Settings.Stores;

public static class UpdateStore
{
    public record Command(Guid Id, StoreInput Input) : IRequest<ErrorOr<StoreDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Input).SetValidator(new StoreInputValidator());
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<StoreDetail>>
    {
        public async Task<ErrorOr<StoreDetail>> Handle(Command command, CancellationToken ct)
        {
            var store = await dbContext.Set<Store>()
                .FirstOrDefaultAsync(s => s.Id == command.Id, ct);

            if (store == null) return StoreErrors.NotFound(command.Id);

            var input = command.Input;
            var result = store.Update(
                input.Name,
                input.Url,
                input.DefaultCurrency,
                input.PricesIncludeTax,
                input.DefaultWeightUnit);

            if (result.IsError) return result.Errors;

            store.SetMetadata(input.PublicMetadata, input.PrivateMetadata);
            if (input.DefaultStockLocationId.HasValue)
            {
                store.DefaultStockLocationId = input.DefaultStockLocationId;
            }

            await dbContext.SaveChangesAsync(ct);
            return store.Adapt<StoreDetail>();
        }
    }
}
