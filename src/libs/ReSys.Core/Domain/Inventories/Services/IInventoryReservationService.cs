using ErrorOr;
using ReSys.Core.Domain.Ordering.LineItems;

namespace ReSys.Core.Domain.Inventories.Services;

/// <summary>
/// Manages the temporary reservation of stock during checkout to prevent overselling.
/// </summary>
public interface IInventoryReservationService
{
    /// <summary>
    /// Attempts to lock stock for the items in the order.
    /// </summary>
    /// <param name="orderId">The order requesting stock.</param>
    /// <param name="items">The items to reserve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if reserved, Error if insufficient stock.</returns>
    Task<ErrorOr<Success>> AttemptReservationAsync(Guid orderId, IEnumerable<LineItem> items, CancellationToken ct);

    /// <summary>
    /// Releases the reservation if payment fails or timeout occurs.
    /// </summary>
    Task ReleaseReservationAsync(Guid orderId, CancellationToken ct);
}
