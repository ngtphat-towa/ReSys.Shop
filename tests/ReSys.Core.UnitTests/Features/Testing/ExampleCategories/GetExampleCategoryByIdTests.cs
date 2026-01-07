using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategoryById;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Features.Testing.ExampleCategories;

public class GetExampleCategoryByIdTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly GetExampleCategoryById.Handler _handler;

    public GetExampleCategoryByIdTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new GetExampleCategoryById.Handler(_context);
    }

    [Fact(DisplayName = "Should return category details when it exists")]
    public async Task Handle_Existing_ShouldReturnDetails()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _context.Set<ExampleCategory>().Add(new ExampleCategory { Id = categoryId, Name = "Existing Cat" });
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetExampleCategoryById.Query(categoryId);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(categoryId);
        result.Value.Name.Should().Be("Existing Cat");
    }

    [Fact(DisplayName = "Should return not found when category does not exist")]
    public async Task Handle_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetExampleCategoryById.Query(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(ExampleCategoryErrors.NotFound);
    }
}
