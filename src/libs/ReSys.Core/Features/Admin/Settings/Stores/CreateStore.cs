using ErrorOr;
using FluentValidation;
using Mapster;
using MediatR;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Features.Admin.Settings.Stores.Common;

namespace ReSys.Core.Features.Admin.Settings.Stores;

public static class CreateStore
{
    public record Command(StoreInput Input) : IRequest<ErrorOr<StoreDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Input).SetValidator(new StoreInputValidator());
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<StoreDetail>>
    {
        public async Task<ErrorOr<StoreDetail>> Handle(Command command, CancellationToken ct)
        {
            var input = command.Input;
            var storeResult = Store.Create(
                input.Name,
                input.Code,
                input.DefaultCurrency,
                input.Url,
                input.PricesIncludeTax);

            if (storeResult.IsError) return storeResult.Errors;

            var store = storeResult.Value;
            store.SetMetadata(input.PublicMetadata, input.PrivateMetadata);
            if (input.DefaultStockLocationId.HasValue)
            {
                store.DefaultStockLocationId = input.DefaultStockLocationId;
            }

            dbContext.Set<Store>().Add(store);
            await dbContext.SaveChangesAsync(ct);

            return store.Adapt<StoreDetail>();
        }
    }
}
