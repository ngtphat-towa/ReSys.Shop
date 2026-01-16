using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Promotions.Promotions;

namespace ReSys.Core.Domain.Promotions.Actions;

public sealed class PromotionAction : Entity
{
    public Guid PromotionId { get; private set; }
    public Promotion.PromotionType Type { get; private set; }

    private PromotionAction() { }

    public static PromotionAction Create(Guid promotionId, Promotion.PromotionType type)
    {
        return new PromotionAction
        {
            PromotionId = promotionId,
            Type = type
        };
    }
}