using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods;

public static class DeleteShippingMethod
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var method = await dbContext.Set<ShippingMethod>()
                .FirstOrDefaultAsync(m => m.Id == command.Id, ct);

            if (method == null) return ShippingMethodErrors.NotFound(command.Id);

            var result = method.Delete();
            if (result.IsError) return result.Errors;

            await dbContext.SaveChangesAsync(ct);
            return Result.Success;
        }
    }
}
