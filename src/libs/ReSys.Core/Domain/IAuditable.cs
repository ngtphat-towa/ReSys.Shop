namespace ReSys.Core.Domain;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    // DateTimeOffset? UpdatedAt { get; set; } // Can be added later if needed
}
