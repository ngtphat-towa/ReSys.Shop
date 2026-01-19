using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Rules;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Admin.Promotions.Rules.AddCategoryRule;

public static class AddCategoryRule
{
    public record Request(List<Guid> TaxonIds, bool IsExclude = false);
    public record Command(Guid PromotionId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.TaxonIds).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .Include(p => p.Rules)
                .FirstOrDefaultAsync(p => p.Id == command.PromotionId, ct);

            if (promotion is null) return PromotionErrors.NotFound(command.PromotionId);

            var req = command.Request;
            var type = req.IsExclude ? PromotionRule.RuleType.CategoryExclude : PromotionRule.RuleType.CategoryInclude;

            var ruleResult = PromotionRule.Create(promotion.Id, type, new RuleParameters { TargetIds = req.TaxonIds });
            if (ruleResult.IsError) return ruleResult.Errors;

            promotion.AddRule(ruleResult.Value);
            context.Set<Promotion>().Update(promotion);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
