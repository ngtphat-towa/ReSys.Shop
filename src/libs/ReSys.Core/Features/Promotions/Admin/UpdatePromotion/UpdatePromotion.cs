using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Promotions;
using ReSys.Core.Features.Promotions.Admin.Common;
using ErrorOr;
using FluentValidation;
using Mapster;

namespace ReSys.Core.Features.Promotions.Admin.UpdatePromotion;

public static class UpdatePromotion
{
    public record Request : PromotionParameters;
    public record Response : PromotionResponse;
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(PromotionConstraints.NameMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var promotion = await context.Set<Promotion>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (promotion is null) return PromotionErrors.NotFound(command.Id);

            var req = command.Request;

            // Domain Logic: Update Aggregate
            var updateResult = promotion.UpdateDetails(
                req.Name, req.Description, req.MinimumOrderAmount, 
                req.MaximumDiscountAmount, req.StartsAt, req.ExpiresAt);

            if (updateResult.IsError) return updateResult.Errors;

            context.Set<Promotion>().Update(promotion);
            await context.SaveChangesAsync(ct);

            return promotion.Adapt<Response>();
        }
    }
}