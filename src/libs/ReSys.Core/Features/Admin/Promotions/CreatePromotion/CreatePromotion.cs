using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Features.Admin.Promotions.Common;
using ErrorOr;
using FluentValidation;
using Mapster;

namespace ReSys.Core.Features.Admin.Promotions.CreatePromotion;

public static class CreatePromotion
{
    public record Request : PromotionParameters;
    public record Response : PromotionResponse;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(PromotionConstraints.NameMaxLength);
            RuleFor(x => x.Request.Code).MaximumLength(PromotionConstraints.CodeMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var req = command.Request;

            // Guard: Name uniqueness
            if (await context.Set<Promotion>().AnyAsync(x => x.Name == req.Name, ct))
                return PromotionErrors.DuplicateName(req.Name);

            // Create Aggregate
            var promotionResult = Promotion.Create(
                req.Name, req.Code, req.Description, req.MinimumOrderAmount, 
                req.MaximumDiscountAmount, req.StartsAt, req.ExpiresAt, 
                req.UsageLimit, req.RequiresCouponCode);

            if (promotionResult.IsError) return promotionResult.Errors;

            var promotion = promotionResult.Value;
            context.Set<Promotion>().Add(promotion);
            await context.SaveChangesAsync(ct);

            return promotion.Adapt<Response>();
        }
    }
}
