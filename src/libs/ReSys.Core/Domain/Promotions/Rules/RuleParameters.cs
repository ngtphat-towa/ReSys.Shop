namespace ReSys.Core.Domain.Promotions.Rules;

/// <summary>
/// Base record for promotion rule configuration.
/// </summary>
public record RuleParameters
{
    public string? Value { get; init; }
    public List<Guid> TargetIds { get; init; } = [];
    public int? Threshold { get; init; }
}
