namespace ReSys.Identity.Domain;

/// <summary>
/// Master catalog of all possible claims/permissions in the system.
/// </summary>
public class ClaimDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Type { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
