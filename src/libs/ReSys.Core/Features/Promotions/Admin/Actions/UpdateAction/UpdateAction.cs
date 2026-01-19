using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Actions;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Promotions.Admin.Actions.UpdateAction;

public static class UpdateAction
{
    public record Request(Promotion.PromotionType Type, ActionParameters Parameters);
    public record Command(Guid PromotionId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Type).NotEmpty().NotEqual(Promotion.PromotionType.None);
            RuleFor(x => x.Request.Parameters).NotNull();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .Include(p => p.Action)
                .FirstOrDefaultAsync(p => p.Id == command.PromotionId, ct);

            if (promotion is null) return PromotionErrors.NotFound(command.PromotionId);

            var req = command.Request;

            if (promotion.Action == null)
            {
                var actionResult = PromotionAction.Create(promotion.Id, req.Type, req.Parameters);
                if (actionResult.IsError) return actionResult.Errors;
                promotion.SetAction(actionResult.Value);
            }
            else
            {
                promotion.Action.Update(req.Parameters);
            }

            context.Set<Promotion>().Update(promotion);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}