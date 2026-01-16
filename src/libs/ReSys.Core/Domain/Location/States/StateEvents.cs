using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Location.States;

public static class StateEvents
{
    public record StateCreated(State State) : IDomainEvent;
    public record StateUpdated(State State) : IDomainEvent;
}
