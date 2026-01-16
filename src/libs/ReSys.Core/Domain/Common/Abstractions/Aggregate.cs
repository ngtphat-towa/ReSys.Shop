namespace ReSys.Core.Domain.Common.Abstractions;

public abstract class Aggregate : Entity, IAggregate
{
    private readonly List<object> _domainEvents = [];

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Records a domain event to be dispatched after the transaction is committed.
    /// </summary>
    protected void RaiseDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);
}
