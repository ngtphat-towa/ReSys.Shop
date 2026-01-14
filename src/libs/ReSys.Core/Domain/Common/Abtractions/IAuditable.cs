namespace ReSys.Core.Domain.Common.Abtractions;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    // DateTimeOffset? UpdatedAt { get; set; } // Can be added later if needed
}
