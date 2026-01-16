using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Promotions.Promotions;

public static class PromotionEvents
{
    public record PromotionCreated(Promotion Promotion) : IDomainEvent;
    public record PromotionUpdated(Promotion Promotion) : IDomainEvent;
    public record PromotionUsed(Promotion Promotion) : IDomainEvent;
}
