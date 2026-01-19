namespace ReSys.Core.Domain.Common.Abstractions;

public interface IHasAssignable
{
    DateTimeOffset? AssignedAt { get; set; }
    string? AssignedBy { get; set; }
    string? AssignedTo { get; set; }
}

public static class HasAssignableExtensions
{
    public static bool IsAssigned(this IHasAssignable? target) =>
        target?.AssignedAt != null && !string.IsNullOrWhiteSpace(target.AssignedTo);

    public static void MarkAsAssigned(this IHasAssignable? target, string assignedTo, string? assignedBy = null)
    {
        if (target == null || string.IsNullOrWhiteSpace(assignedTo)) return;
        target.AssignedAt = DateTimeOffset.UtcNow;
        target.AssignedTo = assignedTo;
        target.AssignedBy = assignedBy;
    }

    public static void MarkAsUnassigned(this IHasAssignable? target)
    {
        if (target == null) return;
        target.AssignedAt = null;
        target.AssignedBy = null;
        target.AssignedTo = null;
    }
}