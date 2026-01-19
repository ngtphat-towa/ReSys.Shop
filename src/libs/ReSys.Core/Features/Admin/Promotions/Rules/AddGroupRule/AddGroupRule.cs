using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Domain.Promotions.Rules;
using ErrorOr;
using FluentValidation;

namespace ReSys.Core.Features.Admin.Promotions.Rules.AddGroupRule;

public static class AddGroupRule
{
    public record Request(List<Guid> GroupIds);
    public record Command(Guid PromotionId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.GroupIds).NotEmpty();
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

            // 1. Verify Groups Exist
            var groupsExist = await context.Set<UserGroup>()
                .Where(g => command.Request.GroupIds.Contains(g.Id))
                .CountAsync(ct) == command.Request.GroupIds.Count;

            if (!groupsExist) return UserGroupErrors.NotFound(Guid.Empty); // Placeholder for generic not found

            // 2. Create Rule
            var ruleResult = PromotionRule.Create(
                promotion.Id, 
                PromotionRule.RuleType.UserGroup, 
                new RuleParameters { TargetIds = command.Request.GroupIds });

            if (ruleResult.IsError) return ruleResult.Errors;

            // 3. Persist
            promotion.AddRule(ruleResult.Value);
            context.Set<Promotion>().Update(promotion);
            await context.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
