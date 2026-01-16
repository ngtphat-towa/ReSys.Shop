using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Catalog.OptionTypes;

public static class OptionTypeEvents
{
    public record OptionTypeCreated(OptionType OptionType) : IDomainEvent;
    public record OptionTypeUpdated(OptionType OptionType) : IDomainEvent;
    public record OptionTypeDeleted(OptionType OptionType) : IDomainEvent;
}
