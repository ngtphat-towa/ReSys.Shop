using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class UpdatePaymentMethodStatus
{
    public record Command(Guid Id, bool IsActive) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

            if (method == null) return PaymentMethodErrors.NotFound(command.Id);

            if (command.IsActive)
                method.Activate();
            else
                method.Deactivate();

            await dbContext.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
