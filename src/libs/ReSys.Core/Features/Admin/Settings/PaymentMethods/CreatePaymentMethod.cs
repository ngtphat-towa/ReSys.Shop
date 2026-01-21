using ErrorOr;
using FluentValidation;
using Mapster;
using MediatR;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class CreatePaymentMethod
{
    public record Command(PaymentMethodInput Input) : IRequest<ErrorOr<PaymentMethodDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Input).SetValidator(new PaymentMethodInputValidator());
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<PaymentMethodDetail>>
    {
        public async Task<ErrorOr<PaymentMethodDetail>> Handle(Command command, CancellationToken ct)
        {
            var input = command.Input;
            var methodResult = PaymentMethod.Create(
                input.Name,
                input.Presentation ?? input.Name,
                input.Type,
                input.Description,
                input.Position,
                input.AutoCapture);

            if (methodResult.IsError) return methodResult.Errors;

            var method = methodResult.Value;
            method.SetMetadata(input.PublicMetadata, input.PrivateMetadata);
            
            dbContext.Set<PaymentMethod>().Add(method);
            await dbContext.SaveChangesAsync(ct);

            return method.Adapt<PaymentMethodDetail>();
        }
    }
}
