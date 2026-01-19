using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering.History;

/// <summary>
/// Represents a single historical event or state transition in an order's lifecycle.
/// Provides a detailed audit trail for support and financial reconciliation.
/// </summary>
public sealed class OrderHistory : Entity
{
    public Guid OrderId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Order.OrderState ToState { get; set; }
    public Order.OrderState? FromState { get; set; }
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// Flexible dictionary for technical audit data (e.g., raw gateway responses, warehouse routing reasons).
    /// Mapped as JSONB in PostgreSQL.
    /// </summary>
    public IDictionary<string, object?> Context { get; set; } = new Dictionary<string, object?>();

    public Order Order { get; set; } = null!;

    private OrderHistory() { }

    public static ErrorOr<OrderHistory> Create(
        Guid orderId,
        string description,
        Order.OrderState toState,
        Order.OrderState? fromState = null,
        string? triggeredBy = "System",
        IDictionary<string, object?>? context = null,
        DateTimeOffset now = default)
    {
        if (string.IsNullOrWhiteSpace(description)) return Error.Validation("OrderHistory.DescriptionRequired", "Description is required.");

        return new OrderHistory
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Description = description,
            ToState = toState,
            FromState = fromState,
            TriggeredBy = triggeredBy,
            Context = context ?? new Dictionary<string, object?>(),
            CreatedAt = now == default ? DateTimeOffset.UtcNow : now
        };
    }
}
