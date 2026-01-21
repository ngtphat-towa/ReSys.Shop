using ErrorOr;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.ShippingMethods;
using ReSys.Core.Features.Admin.Settings.ShippingMethods.Common;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods;

public static class UpdateShippingMethod
{
    public record Command(Guid Id, ShippingMethodInput Input) : IRequest<ErrorOr<ShippingMethodDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Input).SetValidator(new ShippingMethodInputValidator());
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<ShippingMethodDetail>>
    {
        public async Task<ErrorOr<ShippingMethodDetail>> Handle(Command command, CancellationToken ct)
        {
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

            if (method == null) return ShippingMethodErrors.NotFound(command.Id);

            var input = command.Input;
            var result = method.UpdateDetails(
                input.Name,
                input.Presentation ?? input.Name,
                input.BaseCost,
                input.Description,
                input.Position);

            if (result.IsError) return result.Errors;

            method.SetMetadata(input.PublicMetadata, input.PrivateMetadata);

            await dbContext.SaveChangesAsync(ct);
            return method.Adapt<ShippingMethodDetail>();
        }
    }
}