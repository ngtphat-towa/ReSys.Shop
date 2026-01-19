using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Promotions;

/// <summary>
/// Domain events for the Promotions aggregate lifecycle.
/// </summary>
public static class PromotionEvents
{
    public record PromotionCreated(Guid PromotionId, string Name) : IDomainEvent;
    public record PromotionUpdated(Guid PromotionId) : IDomainEvent;
    public record PromotionDeleted(Guid PromotionId, string Name) : IDomainEvent;
    public record PromotionActivated(Guid PromotionId) : IDomainEvent;
    public record PromotionDeactivated(Guid PromotionId) : IDomainEvent;
    public record PromotionUsed(Guid PromotionId, Guid OrderId) : IDomainEvent;
}
