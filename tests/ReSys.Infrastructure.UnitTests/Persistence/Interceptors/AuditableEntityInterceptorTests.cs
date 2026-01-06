using Microsoft.EntityFrameworkCore;


using NSubstitute;


using ReSys.Core.Common.Security;
using ReSys.Core.Domain.Abstractions.Concerns;
using ReSys.Infrastructure.Persistence.Interceptors;

namespace ReSys.Infrastructure.UnitTests.Persistence.Interceptors;

public class AuditableEntityInterceptorTests
{
    private readonly ICurrentUser _currentUser;
    private readonly AuditableEntityInterceptor _sut;
    private readonly TestDbContext _context;
    private const string UserId = "test-user";

    public AuditableEntityInterceptorTests()
    {
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.Id.Returns(UserId);
        
        _sut = new AuditableEntityInterceptor(_currentUser);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_sut)
            .Options;

        _context = new TestDbContext(options);
    }

    [Fact(DisplayName = "SavingChanges: Should set CreatedAt and CreatedBy when a new auditable entity is added")]
    public async Task SavingChanges_ShouldSetAuditProperties_WhenEntityIsAdded()
    {
        // Arrange
        var entity = new TestAuditableEntity { Name = "Test" };

        // Act
        _context.Add(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entity.CreatedBy.Should().Be(UserId);
    }

    [Fact(DisplayName = "SavingChanges: Should update LastModifiedAt and LastModifiedBy when an existing auditable entity is modified")]
    public async Task SavingChanges_ShouldUpdateAuditProperties_WhenEntityIsModified()
    {
        // Arrange
        var entity = new TestAuditableEntity { Name = "Initial" };
        _context.Add(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var initialCreatedAt = entity.CreatedAt;

        // Act
        entity.Name = "Updated";
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entity.CreatedAt.Should().Be(initialCreatedAt);
        entity.LastModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entity.LastModifiedBy.Should().Be(UserId);
    }

    [Fact(DisplayName = "SavingChanges: Should perform a soft delete and set DeletedAt when an ISoftDeletable entity is removed")]
    public async Task SavingChanges_ShouldPerformSoftDelete_WhenEntityIsDeleted()
    {
        // Arrange
        var entity = new TestSoftDeleteEntity { Name = "Delete Me" };
        _context.Add(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        _context.Remove(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        
        var dbEntity = await _context.SoftDeleteEntities.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == entity.Id, TestContext.Current.CancellationToken);
        dbEntity.Should().NotBeNull();
        dbEntity!.IsDeleted.Should().BeTrue();
    }

    [Fact(DisplayName = "SavingChanges: Should increment Version property when an IVersioned entity is modified")]
    public async Task SavingChanges_ShouldIncrementVersion_WhenEntityIsModified()
    {
        // Arrange
        var entity = new TestVersionedEntity { Name = "V1", Version = 1 };
        _context.Add(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        entity.Name = "V2";
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entity.Version.Should().Be(2);
    }

    [Fact(DisplayName = "SavingChanges: Should leave audit properties null when ICurrentUser has no ID")]
    public async Task SavingChanges_ShouldLeaveAuditPropertiesNull_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUser.Id.Returns((string?)null);
        var entity = new TestAuditableEntity { Name = "Test" };

        // Act
        _context.Add(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entity.CreatedBy.Should().BeNull();
    }

    [Fact(DisplayName = "SavingChanges: Should update parent audit properties when an owned entity (child) is modified")]
    public async Task SavingChanges_ShouldUpdateParentAuditProperties_WhenOwnedEntityIsModified()
    {
        // Arrange
        var entity = new TestParentEntity { 
            Name = "Parent", 
            Details = new TestOwnedDetails { Description = "Initial" } 
        };
        _context.Add(entity);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var initialLastModified = entity.LastModifiedAt;

        // Act
        entity.Details.Description = "Changed";
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entity.LastModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        entity.LastModifiedBy.Should().Be(UserId);
    }

    [Fact(DisplayName = "SavingChanges: Should correctly audit multiple entities in a single batch operation")]
    public async Task SavingChanges_ShouldAuditMultipleEntities_InSingleBatch()
    {
        // Arrange
        var entities = Enumerable.Range(0, 5).Select(i => new TestAuditableEntity { Name = $"E{i}" }).ToList();
        _context.AddRange(entities);

        // Act
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        entities.Should().OnlyContain(e => e.CreatedBy == UserId && e.CreatedAt != default);
    }

    private class TestAuditableEntity : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    private class TestSoftDeleteEntity : ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }

    private class TestVersionedEntity : IVersioned
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    private class TestParentEntity : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public TestOwnedDetails Details { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    [Owned]
    private class TestOwnedDetails
    {
        public string Description { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestAuditableEntity> AuditableEntities => Set<TestAuditableEntity>();
        public DbSet<TestSoftDeleteEntity> SoftDeleteEntities => Set<TestSoftDeleteEntity>();
        public DbSet<TestVersionedEntity> VersionedEntities => Set<TestVersionedEntity>();
        public DbSet<TestParentEntity> ParentEntities => Set<TestParentEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestParentEntity>().OwnsOne(p => p.Details);
        }
    }
}