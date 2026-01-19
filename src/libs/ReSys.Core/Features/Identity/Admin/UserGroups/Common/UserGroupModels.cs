namespace ReSys.Core.Features.Identity.Admin.UserGroups.Common;

public record GroupParameters
{
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
}

public record GroupResponse : GroupParameters
{
    public Guid Id { get; init; }
    public bool IsSystemGroup { get; init; }
    public bool IsActive { get; init; }
    public int MemberCount { get; init; }
}