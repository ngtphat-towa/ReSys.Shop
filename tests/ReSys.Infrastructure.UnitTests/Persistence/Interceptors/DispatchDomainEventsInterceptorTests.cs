using MediatR;


using Microsoft.EntityFrameworkCore;


using NSubstitute;


using ReSys.Core.Domain;
using ReSys.Core.Domain.Abstractions.Events;
using ReSys.Infrastructure.Persistence.Interceptors;

namespace ReSys.Infrastructure.UnitTests.Persistence.Interceptors;

public class DispatchDomainEventsInterceptorTests
{
    private readonly IMediator _mediator;
    private readonly DispatchDomainEventsInterceptor _sut;
    private readonly TestDbContext _context;

    public DispatchDomainEventsInterceptorTests()
    {
        _mediator = Substitute.For<IMediator>();
        _sut = new DispatchDomainEventsInterceptor(_mediator);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_sut)
            .Options;

        _context = new TestDbContext(options);
    }

    [Fact(DisplayName = "SavingChanges: Should dispatch a domain event when an entity has one event")]
    public async Task SavingChanges_ShouldDispatchDomainEvents_WhenEntityHasEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);
        _context.Entities.Add(entity);

        // Act
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await _mediator.Received(1).Publish(domainEvent, Arg.Any<CancellationToken>());
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "SavingChanges: Should dispatch all domain events when an entity has multiple events")]
    public async Task SavingChanges_ShouldDispatchMultipleDomainEvents_WhenEntityHasMultipleEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        entity.AddDomainEvent(event1);
        entity.AddDomainEvent(event2);
        _context.Entities.Add(entity);

        // Act
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await _mediator.Received(1).Publish(event1, Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(event2, Arg.Any<CancellationToken>());
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "SavingChanges: Should dispatch events from multiple entities in the same save operation")]
    public async Task SavingChanges_ShouldDispatchEventsFromMultipleEntities_InSingleSave()
    {
        // Arrange
        var entity1 = new TestEntity();
        var event1 = new TestDomainEvent();
        entity1.AddDomainEvent(event1);

        var entity2 = new TestEntity();
        var event2 = new TestDomainEvent();
        entity2.AddDomainEvent(event2);

        _context.Entities.AddRange(entity1, entity2);

        // Act
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await _mediator.Received(1).Publish(event1, Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(event2, Arg.Any<CancellationToken>());
        entity1.DomainEvents.Should().BeEmpty();
        entity2.DomainEvents.Should().BeEmpty();
    }

    [Fact(DisplayName = "SavingChanges: Should not call Mediator Publish when no entities have domain events")]
    public async Task SavingChanges_ShouldNotDispatch_WhenEntityHasNoEvents()
    {
        // Arrange
        var entity = new TestEntity();
        _context.Entities.Add(entity);

        // Act
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await _mediator.DidNotReceive().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    private class TestDomainEvent : IDomainEvent { }

    private class TestEntity : Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }
}