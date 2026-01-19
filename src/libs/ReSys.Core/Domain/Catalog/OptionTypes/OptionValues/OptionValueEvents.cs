using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

public static class OptionValueEvents
{
    public record OptionValueCreated(OptionValue OptionValue) : IDomainEvent;
    public record OptionValueUpdated(OptionValue OptionValue) : IDomainEvent;
    public record OptionValueDeleted(OptionValue OptionValue) : IDomainEvent;
}
