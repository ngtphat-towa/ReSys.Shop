using ReSys.Core.UnitTests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Features.Testing.Examples.CreateExample;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Domain.Testing.ExampleCategories;

namespace ReSys.Core.UnitTests.Features.Testing.Examples;

[Trait("Category", "Unit")]
[Trait("Module", "Core")]
[Trait("Feature", "Examples")]
public class CreateExampleTests : IClassFixture<TestDatabaseFixture>
{
    private readonly IApplicationDbContext _context;
    private readonly CreateExample.Handler _handler;

    public CreateExampleTests(TestDatabaseFixture fixture)
    {
        _context = fixture.Context;
        _handler = new CreateExample.Handler(_context);
    }

    [Fact(DisplayName = "Handle: Should successfully create an example and save it to the database when the request is valid")]
    public async Task Handle_ValidRequest_CreatesExample()
    {
        // Arrange
        var uniqueName = $"Example_{Guid.NewGuid()}";
        var request = new CreateExample.Request
        {
            Name = uniqueName,
            Description = "A valid example description",
            Price = 99.99m,
            ImageUrl = "http://example.com/image.jpg",
            Status = ExampleStatus.Active,
            HexColor = "#FF5733"
        };
        var command = new CreateExample.Command(request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse("because the request is valid and the name is unique");
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(uniqueName);
        result.Value.Status.Should().Be(ExampleStatus.Active);
        result.Value.HexColor.Should().Be("#FF5733");

        var dbExample = await _context.Set<Example>().FindAsync([result.Value.Id], TestContext.Current.CancellationToken);
        dbExample.Should().NotBeNull("because the example should be persisted in the database");
        dbExample!.Status.Should().Be(ExampleStatus.Active);
        dbExample.HexColor.Should().Be("#FF5733");
    }

    [Fact(DisplayName = "Handle: Should successfully create an example with a category")]
    public async Task Handle_WithCategory_CreatesExample()
    {
        // Arrange
        var category = new ExampleCategory { Id = Guid.NewGuid(), Name = $"Cat_{Guid.NewGuid()}" };
        _context.Set<ExampleCategory>().Add(category);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var uniqueName = $"Example_{Guid.NewGuid()}";
        var request = new CreateExample.Request
        {
            Name = uniqueName,
            Price = 10,
            CategoryId = category.Id
        };
        var command = new CreateExample.Command(request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CategoryId.Should().Be(category.Id);
        result.Value.CategoryName.Should().Be(category.Name);

        var dbExample = await _context.Set<Example>().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == result.Value.Id, TestContext.Current.CancellationToken);
        dbExample!.CategoryId.Should().Be(category.Id);
        dbExample.Category!.Name.Should().Be(category.Name);
    }

    [Fact(DisplayName = "Handle: Should return a conflict error when attempting to create an example with a name that already exists")]
    public async Task Handle_NameAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var duplicateName = $"Duplicate_{Guid.NewGuid()}";
        _context.Set<Example>().Add(new Example { Id = Guid.NewGuid(), Name = duplicateName, Description = "Existing", Price = 10 });
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateExample.Request { Name = duplicateName, Description = "New", Price = 20 };
        var command = new CreateExample.Command(request);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeTrue("because an example with the same name already exists");
        result.FirstError.Should().BeEquivalentTo(ExampleErrors.DuplicateName);
    }
}


