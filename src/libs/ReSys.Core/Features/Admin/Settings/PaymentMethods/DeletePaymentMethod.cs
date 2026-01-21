using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class DeletePaymentMethod
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var method = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

            if (method == null) return PaymentMethodErrors.NotFound(command.Id);

            var result = method.Delete();
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return Result.Deleted;
        }
    }
}
