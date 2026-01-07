using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.UpdateExampleCategory;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Testing.ExampleCategories;

public class UpdateExampleCategoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly UpdateExampleCategory.Handler _handler;

    public UpdateExampleCategoryTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new UpdateExampleCategory.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully update an existing category")]
    public async Task Handle_ValidRequest_ShouldUpdateCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _context.Set<ExampleCategory>().Add(new ExampleCategory { Id = categoryId, Name = "Old Name" });
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateExampleCategory.Request { Name = "New Name", Description = "New Desc" };
        var command = new UpdateExampleCategory.Command(categoryId, request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("New Name");
        result.Value.Description.Should().Be("New Desc");
    }

    [Fact(DisplayName = "Should return not found when category does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateExampleCategory.Request { Name = "New Name" };
        var command = new UpdateExampleCategory.Command(Guid.NewGuid(), request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(ExampleCategoryErrors.NotFound);
    }

    [Fact(DisplayName = "Should allow updating a category with its current name")]
    public async Task Handle_SameName_ShouldNotReturnConflict()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var name = "Stable Name";
        _context.Set<ExampleCategory>().Add(new ExampleCategory { Id = categoryId, Name = name });
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateExampleCategory.Request { Name = name, Description = "Updated Desc" };
        var command = new UpdateExampleCategory.Command(categoryId, request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(name);
    }

    [Fact(DisplayName = "Should return conflict when updating to a name that already exists")]
    public async Task Handle_DuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var cat1Id = Guid.NewGuid();
        var cat2Id = Guid.NewGuid();
        _context.Set<ExampleCategory>().AddRange(
            new ExampleCategory { Id = cat1Id, Name = "Existing" },
            new ExampleCategory { Id = cat2Id, Name = "Other" }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateExampleCategory.Request { Name = "Existing" };
        var command = new UpdateExampleCategory.Command(cat2Id, request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(ExampleCategoryErrors.DuplicateName);
    }
}
