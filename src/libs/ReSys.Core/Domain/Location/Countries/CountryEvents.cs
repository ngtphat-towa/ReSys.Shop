using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Location.Countries;

public static class CountryEvents
{
    public record CountryCreated(Country Country) : IDomainEvent;
    public record CountryUpdated(Country Country) : IDomainEvent;
}
