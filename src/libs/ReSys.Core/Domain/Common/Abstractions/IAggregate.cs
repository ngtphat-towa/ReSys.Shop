namespace ReSys.Core.Domain.Common.Abstractions;

public interface IAggregate
{
    IReadOnlyCollection<object> DomainEvents { get; }
    void ClearDomainEvents();
}
