using MediatR;

namespace ReSys.Core.Domain.Abstractions.Events;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent : INotification;
