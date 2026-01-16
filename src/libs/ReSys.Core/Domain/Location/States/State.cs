using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Location.States;

public sealed class State : Aggregate
{
    public string Name { get; set; } = string.Empty;
    public string? Abbr { get; set; }
    public Guid CountryId { get; set; }

    public State() { }

    public static ErrorOr<State> Create(string name, string? abbr, Guid countryId)
    {
        if (string.IsNullOrWhiteSpace(name)) return StateErrors.NameRequired;
        if (name.Length > StateConstraints.NameMaxLength) return StateErrors.NameTooLong;
        if (countryId == Guid.Empty) return StateErrors.CountryIdRequired;
        if (abbr?.Length > StateConstraints.AbbrMaxLength) return StateErrors.AbbrTooLong;

        var state = new State
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Abbr = abbr?.Trim().ToUpperInvariant(),
            CountryId = countryId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        state.RaiseDomainEvent(new StateEvents.StateCreated(state));
        return state;
    }

    public ErrorOr<Success> Update(string name, string? abbr, Guid countryId)
    {
        if (string.IsNullOrWhiteSpace(name)) return StateErrors.NameRequired;
        if (name.Length > StateConstraints.NameMaxLength) return StateErrors.NameTooLong;
        if (countryId == Guid.Empty) return StateErrors.CountryIdRequired;
        if (abbr?.Length > StateConstraints.AbbrMaxLength) return StateErrors.AbbrTooLong;

        Name = name.Trim();
        Abbr = abbr?.Trim().ToUpperInvariant();
        CountryId = countryId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new StateEvents.StateUpdated(this));
        return Result.Success;
    }
}
