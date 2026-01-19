using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ErrorOr;

namespace ReSys.Core.Features.Admin.Promotions.ActivatePromotion;

public static class ActivatePromotion
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

            if (promotion is null) return PromotionErrors.NotFound(command.Id);

            var result = promotion.Activate();
            if (result.IsError) return result.Errors;

            context.Set<Promotion>().Update(promotion);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
