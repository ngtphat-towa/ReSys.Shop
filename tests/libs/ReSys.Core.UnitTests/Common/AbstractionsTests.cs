using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.UnitTests.Common;

[Trait("Category", "Unit")]
[Trait("Module", "Common")]
public class AbstractionsTests
{
    private class TestEntity : Entity { }
    private class TestAggregate : Aggregate 
    {
        public void AddTestEvent(object @event) => RaiseDomainEvent(@event);
    }

    [Fact(DisplayName = "Entity should initialize with new ID and Version 0")]
    public void Entity_ShouldInitialize_WithDefaults()
    {
        var entity = new TestEntity();
        entity.Id.Should().NotBeEmpty();
        entity.Version.Should().Be(0);
    }

    [Fact(DisplayName = "Aggregate should manage domain events")]
    public void Aggregate_ShouldManage_DomainEvents()
    {
        var aggregate = new TestAggregate();
        var testEvent = new { Name = "Test" };

        aggregate.AddTestEvent(testEvent);
        aggregate.DomainEvents.Should().ContainSingle().Which.Should().Be(testEvent);

        aggregate.ClearDomainEvents();
        aggregate.DomainEvents.Should().BeEmpty();
    }
}
