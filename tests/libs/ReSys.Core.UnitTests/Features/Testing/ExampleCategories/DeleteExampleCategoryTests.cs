using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Features.Testing.ExampleCategories.DeleteExampleCategory;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Testing.ExampleCategories;

public class DeleteExampleCategoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly DeleteExampleCategory.Handler _handler;

    public DeleteExampleCategoryTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new DeleteExampleCategory.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully delete an existing category")]
    public async Task Handle_ValidRequest_ShouldDeleteCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _context.Set<ExampleCategory>().Add(new ExampleCategory { Id = categoryId, Name = "To Delete" });
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new DeleteExampleCategory.Command(categoryId);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        
        var dbCategory = await _context.Set<ExampleCategory>().FindAsync([categoryId], TestContext.Current.CancellationToken);
        dbCategory.Should().BeNull();
    }

    [Fact(DisplayName = "Should set CategoryId to null on Examples when category is deleted")]
    public async Task Handle_CategoryWithExamples_ShouldNullifyReferences()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new ExampleCategory { Id = categoryId, Name = "Parent" };
        var exampleId = Guid.NewGuid();
        var example = new Example { Id = exampleId, Name = "Child", Price = 10, CategoryId = categoryId };

        _context.Set<ExampleCategory>().Add(category);
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new DeleteExampleCategory.Command(categoryId);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var dbExample = await _context.Set<Example>().FindAsync([exampleId], TestContext.Current.CancellationToken);
        dbExample.Should().NotBeNull();
        dbExample!.CategoryId.Should().BeNull();
    }

    [Fact(DisplayName = "Should return not found when category does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DeleteExampleCategory.Command(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(ExampleCategoryErrors.NotFound);
    }
}
