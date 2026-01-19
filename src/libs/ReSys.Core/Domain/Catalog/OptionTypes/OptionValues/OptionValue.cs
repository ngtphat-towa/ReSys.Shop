using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

/// <summary>
/// Represents a specific value for an option type (e.g., 'Red' for 'Color').
/// Glue entity within the OptionType aggregate.
/// </summary>
public sealed class OptionValue : Aggregate
{
    public Guid OptionTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public int Position { get; set; }

    public OptionType OptionType { get; set; } = null!;

    public OptionValue() { }

    /// <summary>
    /// Factory for creating a new option value.
    /// </summary>
    public static ErrorOr<OptionValue> Create(Guid optionTypeId, string name, string? presentation = null, int position = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionValueErrors.NameRequired;
        if (name.Length > OptionValueConstraints.NameMaxLength) return OptionValueErrors.NameTooLong;

        var finalPresentation = presentation?.Trim() ?? name.Trim();
        if (finalPresentation.Length > OptionValueConstraints.PresentationMaxLength) return OptionValueErrors.PresentationTooLong;

        return new OptionValue
        {
            Id = Guid.NewGuid(),
            OptionTypeId = optionTypeId,
            Name = name.Trim(),
            Presentation = finalPresentation,
            Position = position,
            CreatedAt = DateTimeOffset.UtcNow
        };
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
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new OptionValueEvents.OptionValueUpdated(this));
        return Result.Success;
    }

    public ErrorOr<Deleted> Delete()
    {
        RaiseDomainEvent(new OptionValueEvents.OptionValueDeleted(this));
        return Result.Deleted;
    }
}
