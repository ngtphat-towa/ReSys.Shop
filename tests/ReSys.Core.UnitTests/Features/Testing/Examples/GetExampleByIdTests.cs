using ReSys.Core.UnitTests.TestInfrastructure;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Features.Testing.Examples.GetExampleById;
using ReSys.Core.Domain;
using ReSys.Core.Common.Data;

namespace ReSys.Core.UnitTests.Features.Testing.Examples;

public class GetExampleByIdTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly GetExampleById.Handler _handler;

    public GetExampleByIdTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new GetExampleById.Handler(_context);
    }

    [Fact(DisplayName = "Should return the correct example details when an example with the specified ID exists")]
    public async Task Handle_ExistingExample_ShouldReturnDetails()
    {
        // Arrange
        var exampleId = Guid.NewGuid();
        var example = new Example
        {
            Id = exampleId,
            Name = $"TestExample_{Guid.NewGuid()}",
            Description = "Full Description",
            Price = 49.99m,
            Status = ExampleStatus.Active,
            HexColor = "#123456",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new GetExampleById.Request(exampleId);
        var query = new GetExampleById.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse("because the example exists");
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(exampleId);
        result.Value.Name.Should().Be(example.Name);
        result.Value.Status.Should().Be(ExampleStatus.Active);
        result.Value.HexColor.Should().Be("#123456");
    }

    [Fact(DisplayName = "Should return example details with category name when it has one")]
    public async Task Handle_WithCategory_ShouldReturnCategoryName()
    {
        // Arrange
        var category = new ExampleCategory { Id = Guid.NewGuid(), Name = "MyCategory" };
        var exampleId = Guid.NewGuid();
        var example = new Example
        {
            Id = exampleId,
            Name = "CategorizedExample",
            Price = 10,
            CategoryId = category.Id
        };

        _context.Set<ExampleCategory>().Add(category);
        _context.Set<Example>().Add(example);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetExampleById.Query(new GetExampleById.Request(exampleId));

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CategoryId.Should().Be(category.Id);
        result.Value.CategoryName.Should().Be("MyCategory");
    }

    [Fact(DisplayName = "Should return a not found error when searching for an example ID that does not exist")]
    public async Task Handle_NonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetExampleById.Request(nonExistentId);
        var query = new GetExampleById.Query(request);

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue("because no example exists with the given ID");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.NotFound(nonExistentId));
    }
}


