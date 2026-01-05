namespace ReSys.Core.Domain.Abstractions.Concerns;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
