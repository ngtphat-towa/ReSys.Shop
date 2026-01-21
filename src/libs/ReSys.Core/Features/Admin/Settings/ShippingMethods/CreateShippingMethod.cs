using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.ShippingMethods;
using ReSys.Core.Features.Admin.Settings.ShippingMethods.Common;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods;

public static class CreateShippingMethod
{
    public record Command(ShippingMethodInput Input) : IRequest<ErrorOr<ShippingMethodDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Input).SetValidator(new ShippingMethodInputValidator());
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<ShippingMethodDetail>>
    {
        public async Task<ErrorOr<ShippingMethodDetail>> Handle(Command command, CancellationToken ct)
        {
            var input = command.Input;
            var methodResult = ShippingMethod.Create(
                input.Name,
                input.Presentation ?? input.Name,
                input.Type,
                input.BaseCost,
                input.Description,
                input.Position);

            if (methodResult.IsError) return methodResult.Errors;

            var method = methodResult.Value;
            method.SetMetadata(input.PublicMetadata, input.PrivateMetadata);

            dbContext.Set<ShippingMethod>().Add(method);
            await dbContext.SaveChangesAsync(ct);

            return method.Adapt<ShippingMethodDetail>();
        }
    }
}