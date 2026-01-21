using ErrorOr;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class UpdatePaymentMethod
{
    public record Command(Guid Id, PaymentMethodInput Input) : IRequest<ErrorOr<PaymentMethodDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Input).SetValidator(new PaymentMethodInputValidator());
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<PaymentMethodDetail>>
    {
        public async Task<ErrorOr<PaymentMethodDetail>> Handle(Command command, CancellationToken ct)
        {
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

            if (method == null) return PaymentMethodErrors.NotFound(command.Id);

            var input = command.Input;
            var result = method.UpdateDetails(
                input.Name,
                input.Presentation ?? input.Name,
                input.Description,
                input.Position,
                input.AutoCapture);

            if (result.IsError) return result.Errors;

            method.SetMetadata(input.PublicMetadata, input.PrivateMetadata);

            await dbContext.SaveChangesAsync(ct);
            return method.Adapt<PaymentMethodDetail>();
        }
    }
}
