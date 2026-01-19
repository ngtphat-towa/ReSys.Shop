using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Settings.Stores;

public static class StoreEvents
{
    public record StoreCreated(Store Store) : IDomainEvent;
    public record StoreUpdated(Store Store) : IDomainEvent;
    public record StoreDeleted(Store Store) : IDomainEvent;
}
