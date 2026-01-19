using ErrorOr;

namespace ReSys.Infrastructure.Persistence.Seeders;

/// <summary>
/// Defines a contract for database seeding operations.
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Executes the seeding logic.
    /// </summary>
    Task<ErrorOr<Success>> SeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Execution order (lower numbers run first).
    /// </summary>
    int Order { get; }
}