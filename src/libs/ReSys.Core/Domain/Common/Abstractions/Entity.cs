namespace ReSys.Core.Domain.Common.Abstractions;

public abstract class Entity : IAuditable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public uint Version { get; set; }

    // IAuditable implementation
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}