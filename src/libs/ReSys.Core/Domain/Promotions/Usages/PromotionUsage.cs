using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Promotions.Usages;

public sealed class PromotionUsage : Entity
{
    public Guid PromotionId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? UserId { get; private set; }
    public string? UserEmail { get; private set; }

    private PromotionUsage() { }

    public static PromotionUsage Create(Guid promotionId, string action, string description, string? userId = null, string? userEmail = null)
    {
        return new PromotionUsage
        {
            PromotionId = promotionId,
            Action = action,
            Description = description,
            UserId = userId,
            UserEmail = userEmail
        };
    }
}
