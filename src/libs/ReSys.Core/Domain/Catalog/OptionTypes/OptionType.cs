using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

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

    public static ErrorOr<OptionType> Create(string name, string? presentation = null, int position = 0, bool filterable = false)
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

    public ErrorOr<OptionValue> AddValue(string name, string? presentation = null)
    {
        if (OptionValues.Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return OptionValueErrors.NameAlreadyExists(name);

        var nextPosition = OptionValues.Any() ? OptionValues.Max(v => v.Position) + 1 : 0;

        var valueResult = OptionValue.Create(Id, name, presentation, nextPosition);
        if (valueResult.IsError) return valueResult.Errors;

        OptionValues.Add(valueResult.Value);
        return valueResult.Value;
    }

    public void RemoveValue(Guid valueId)
    {
        var value = OptionValues.FirstOrDefault(v => v.Id == valueId);
        if (value != null)
        {
            OptionValues.Remove(value);
        }
    }
}
