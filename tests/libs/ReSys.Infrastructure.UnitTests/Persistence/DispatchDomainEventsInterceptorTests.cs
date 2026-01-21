using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using ReSys.Core.Domain.Common.Abstractions;
using ReSys.Infrastructure.Persistence.Interceptors;
using FluentAssertions;

namespace ReSys.Infrastructure.UnitTests.Persistence;

public class DispatchDomainEventsInterceptorTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly DispatchDomainEventsInterceptor _sut;

    public DispatchDomainEventsInterceptorTests()
    {
        _sut = new DispatchDomainEventsInterceptor(_mediator);
    }

    [Fact(DisplayName = "DispatchDomainEvents: Should publish events and clear them from entities")]
    public async Task DispatchDomainEvents_Should_PublishAndClearEvents()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_sut)
            .Options;

        using var context = new TestDbContext(options);
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent();
        aggregate.AddEvent(domainEvent);
        
        // Ensure entity is tracked
        await context.Aggregates.AddAsync(aggregate, TestContext.Current.CancellationToken);

        // Act
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await _mediator.Received(1).Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
        aggregate.DomainEvents.Should().BeEmpty();
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregate>().HasKey(x => x.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private class TestAggregate : IAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        private readonly List<object> _domainEvents = [];
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        public void AddEvent(object domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    private record TestDomainEvent : INotification;
}
