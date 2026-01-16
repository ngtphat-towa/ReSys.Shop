using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Catalog.OptionTypes;

public sealed class OptionValue : Entity, IHasMetadata
{
    public Guid OptionTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Presentation { get; set; } = string.Empty;
    public int Position { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public OptionType OptionType { get; set; } = null!;

    private OptionValue() { }

    public static ErrorOr<OptionValue> Create(Guid optionTypeId, string name, string? presentation = null, int position = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionValueErrors.NameRequired;

        return new OptionValue
        {
            OptionTypeId = optionTypeId,
            Name = name.Trim(),
            Presentation = presentation?.Trim() ?? name.Trim(),
            Position = position
        };
    }

    public ErrorOr<Success> Update(string name, string presentation, int position)
    {
        if (string.IsNullOrWhiteSpace(name)) return OptionValueErrors.NameRequired;

        Name = name.Trim();
        Presentation = presentation.Trim();
        Position = position;

        return Result.Success;
    }
}