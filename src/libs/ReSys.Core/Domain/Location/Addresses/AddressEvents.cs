using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Location.Addresses;

public static class AddressEvents
{
    // Note: Address is often used as a Value Object within other Aggregates.
    // These events are available if Address is tracked independently or for audit trails.
    public record AddressCreated(Address Address) : IDomainEvent;
    public record AddressUpdated(Address Address) : IDomainEvent;
}
