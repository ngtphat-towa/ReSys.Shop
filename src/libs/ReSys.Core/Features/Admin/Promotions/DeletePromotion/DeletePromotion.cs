using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ErrorOr;

namespace ReSys.Core.Features.Admin.Promotions.DeletePromotion;

public static class DeletePromotion
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (promotion is null) return PromotionErrors.NotFound(command.Id);

            // Domain Logic: Soft delete handled via aggregate or context
            context.Set<Promotion>().Remove(promotion);
            await context.SaveChangesAsync(ct);

            return Result.Deleted;
        }
    }
}
