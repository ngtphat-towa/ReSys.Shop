using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.PropertyTypes;

public static class PropertyTypeEvents
{
    public record PropertyTypeCreated(PropertyType PropertyType) : IDomainEvent;
    public record PropertyTypeUpdated(PropertyType PropertyType) : IDomainEvent;
    public record PropertyTypeDeleted(PropertyType PropertyType) : IDomainEvent;
}
