namespace ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;

// Parameters:
public record OptionValueParameters
{
    public string Name { get; init; } = null!;
    public string Presentation { get; init; } = null!;
    public int Position { get; init; }
}

// Input:
public record OptionValueInput : OptionValueParameters;

// Read:
public record OptionValueModel : OptionValueParameters
{
    public Guid Id { get; init; }
}
