using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Testing.ExampleCategories;

public class CreateExampleCategoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly CreateExampleCategory.Handler _handler;

    public CreateExampleCategoryTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new CreateExampleCategory.Handler(_context);
    }

    [Fact(DisplayName = "Should successfully create an example category and save it to the database when the request is valid")]
    public async Task Handle_ValidRequest_ShouldCreateCategory()
    {
        // Arrange
        var uniqueName = $"Category_{Guid.NewGuid()}";
        var request = new CreateExampleCategory.Request
        {
            Name = uniqueName,
            Description = "A valid category description"
        };
        var command = new CreateExampleCategory.Command(request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(uniqueName);

        var dbCategory = await _context.Set<ExampleCategory>().FindAsync([result.Value.Id], TestContext.Current.CancellationToken);
        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(uniqueName);
    }

    [Fact(DisplayName = "Should return a conflict error when attempting to create a category with a name that already exists")]
    public async Task Handle_DuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var duplicateName = $"Duplicate_{Guid.NewGuid()}";
        _context.Set<ExampleCategory>().Add(new ExampleCategory { Id = Guid.NewGuid(), Name = duplicateName });
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateExampleCategory.Request { Name = duplicateName };
        var command = new CreateExampleCategory.Command(request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(ExampleCategoryErrors.DuplicateName);
    }
}
