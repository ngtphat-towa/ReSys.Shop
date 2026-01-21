using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class UpdatePaymentMethodConfig
{
    public record Command(Guid Id, Dictionary<string, string> Secrets) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Secrets).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

            if (method == null) return Error.NotFound("PaymentMethod.NotFound");

            foreach (var secret in command.Secrets)
            {
                // Only update if value is provided (allows partial updates)
                if (!string.IsNullOrWhiteSpace(secret.Value))
                {
                    method.PrivateMetadata[secret.Key] = secret.Value;
                }
            }

            await dbContext.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
