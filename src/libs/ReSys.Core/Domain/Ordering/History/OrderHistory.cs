using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Ordering.History;

/// <summary>
/// Represents a single historical event or state transition in an order's lifecycle.
/// It provides a detailed, immutable audit trail for customer support, financial reconciliation, 
/// and technical troubleshooting (e.g. gateway logs).
/// </summary>
public sealed class OrderHistory : Entity
{
    #region Properties
    /// <summary>The parent order reference.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Human-readable explanation of what happened (e.g. 'Payment Captured').</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>The state resulting from this historical event.</summary>
    public Order.OrderState ToState { get; set; }

    /// <summary>The previous state before this event occurred.</summary>
    public Order.OrderState? FromState { get; set; }

    /// <summary>The actor who triggered this change (e.g. 'System', 'Admin:123', 'Webhook').</summary>
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// Flexible dictionary for technical audit data (e.g., raw gateway responses, warehouse routing reasons).
    /// This allows storing structured data without changing the schema.
    /// </summary>
    public IDictionary<string, object?> Context { get; set; } = new Dictionary<string, object?>();

    // Relationships
    /// <summary>Parent order navigation.</summary>
    public Order Order { get; set; } = null!;
    #endregion

    private OrderHistory() { }

    /// <summary>
    /// Factory for creating a new historical audit entry.
    /// </summary>
    public static ErrorOr<OrderHistory> Create(
        Guid orderId,
        string description,
        Order.OrderState toState,
        Order.OrderState? fromState = null,
        string? triggeredBy = "System",
        IDictionary<string, object?>? context = null,
        DateTimeOffset now = default)
    {
        // Guard: Traceability requires a meaningful description.
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
