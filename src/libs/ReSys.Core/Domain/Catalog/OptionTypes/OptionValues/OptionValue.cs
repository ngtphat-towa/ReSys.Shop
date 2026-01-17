using ReSys.Core.Domain.Common.Abstractions;

using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

public sealed class OptionValue : Aggregate
{
    public Guid OptionTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public int Position { get; set; }

    public OptionType OptionType { get; set; } = null!;

    private OptionValue() { }

    internal static ErrorOr<OptionValue> Create(Guid optionTypeId, string name, string? presentation = null, int position = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionValueErrors.NameRequired;
        if (name.Length > OptionValueConstraints.NameMaxLength) return OptionValueErrors.NameTooLong;

        var finalPresentation = presentation?.Trim() ?? name.Trim();
        if (finalPresentation.Length > OptionValueConstraints.PresentationMaxLength) return OptionValueErrors.PresentationTooLong;

        var optionValue = new OptionValue
        {
            OptionTypeId = optionTypeId,
            Name = name.Trim(),
            Presentation = finalPresentation,
            Position = position
        };

        // Note: Created event is still best raised by the factory (OptionType) 
        // to ensure it was added to a valid collection.
        return optionValue;
    }

    public ErrorOr<Success> Update(string name, string presentation, int position)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionValueErrors.NameRequired;
        if (name.Length > OptionValueConstraints.NameMaxLength) return OptionValueErrors.NameTooLong;

        if (string.IsNullOrWhiteSpace(presentation)) return OptionValueErrors.PresentationRequired;
        if (presentation.Length > OptionValueConstraints.PresentationMaxLength) return OptionValueErrors.PresentationTooLong;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Position = position;

        // Self-published event (Relaxed AR rule)
        RaiseDomainEvent(new OptionValueEvents.OptionValueUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        // Self-published event (Relaxed AR rule)
        RaiseDomainEvent(new OptionValueEvents.OptionValueDeleted(this));
        return Result.Deleted;
    }
}
