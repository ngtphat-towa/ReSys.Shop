using MediatR;

namespace ReSys.Core.Domain.Abstractions.Events;

/// <summary>
/// Defines a handler for a domain event of type <typeparamref name="TEvent"/>.
/// </summary>
/// <typeparam name="TEvent">The type of the domain event being handled.</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent;
