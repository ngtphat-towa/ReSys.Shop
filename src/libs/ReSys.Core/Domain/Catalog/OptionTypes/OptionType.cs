using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Domain.Catalog.OptionTypes;

public sealed class OptionType : Aggregate, IHasMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool Filterable { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public ICollection<OptionValue> OptionValues { get; set; } = new List<OptionValue>();

    private OptionType() { }

    public static ErrorOr<OptionType> Create(
        string name,
        string? presentation = null,
        int position = 0,
        bool filterable = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionTypeErrors.NameRequired;

        var optionType = new OptionType
        {
            Name = name.Trim(),
            Presentation = presentation?.Trim() ?? name.Trim(),
            Position = position,
            Filterable = filterable
        };

        optionType.RaiseDomainEvent(new OptionTypeEvents.OptionTypeCreated(optionType));
        return optionType;
    }

    public ErrorOr<Success> Update(
        string name,
        string presentation,
        int position,
        bool filterable)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionTypeErrors.NameRequired;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Position = position;
        Filterable = filterable;

        RaiseDomainEvent(new OptionTypeEvents.OptionTypeUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        if (OptionValues.Any())
        {
            return OptionTypeErrors.CannotDeleteWithValues;
        }

        RaiseDomainEvent(new OptionTypeEvents.OptionTypeDeleted(this));
        return Result.Deleted;
    }

    #region Option Value Management

    /// <summary>
    /// Factory method to add a new value while enforcing uniqueness within the aggregate.
    /// </summary>
    public ErrorOr<OptionValue> AddValue(string name, string? presentation = null)
    {
        if (OptionValues.Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return OptionValueErrors.NameAlreadyExists(name);

        var nextPosition = OptionValues.Any() ? OptionValues.Max(v => v.Position) + 1 : 0;

        var valueResult = OptionValue.Create(Id, name, presentation, nextPosition);
        if (valueResult.IsError) return valueResult.Errors;

        var value = valueResult.Value;
        OptionValues.Add(value);
        
        // Root publishes the creation event
        RaiseDomainEvent(new OptionValueEvents.OptionValueCreated(value));
        return value;
    }

    /// <summary>
    /// Manages the collection-level invariant of ordering.
    /// </summary>
    public ErrorOr<Success> ReorderValues(IEnumerable<(Guid Id, int Position)> newPositions)
    {
        foreach (var pos in newPositions)
        {
            var value = OptionValues.FirstOrDefault(v => v.Id == pos.Id);
            if (value != null)
            {
                value.Position = pos.Position;
            }
        }

        RaiseDomainEvent(new OptionTypeEvents.OptionTypeUpdated(this));
        return Result.Success;
    }

    #endregion
}
