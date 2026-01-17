using Carter;

using Microsoft.AspNetCore.Mvc;

using MediatR;

using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;
using ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategories;
using ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategoryById;
using ReSys.Core.Features.Testing.ExampleCategories.UpdateExampleCategory;
using ReSys.Core.Features.Testing.ExampleCategories.DeleteExampleCategory;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.ExampleCategories;

/// <summary>
/// Module for managing example categories.
/// </summary>
public class ExampleCategoriesModule : ICarterModule
{
    /// <summary>
    /// Registers endpoints for example category management.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // API: Group all example-category-related endpoints under a common prefix
        var group = app.MapGroup("/api/testing/example-categories")
            .WithTags("Example Categories");

        // API: Retrieve a paginated list of example categories
        group.MapGet("/", async ([AsParameters] GetExampleCategories.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetExampleCategories.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetExampleCategories");

        // API: Retrieve a single example category by its identifier
        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExampleCategoryById.Query(id));
            return result.ToApiResponse();
        })
        .WithName("GetExampleCategoryById");

        // API: Create a new example category
        group.MapPost("/", async (
            [FromBody] CreateExampleCategory.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateExampleCategory.Command(request), ct);
            return result.ToApiCreatedResponse(category => $"/api/testing/example-categories/{category.Id}");
        })
        .WithName("CreateExampleCategory");

        // API: Update an existing example category
        group.MapPut("/{id}", async (
            Guid id,
            [FromBody] UpdateExampleCategory.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateExampleCategory.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateExampleCategory");

        // BUS: Ensure cascading delete logic or constraints are handled via the command
        // API: Delete an example category by its identifier
        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteExampleCategory.Command(id));

            if (result.IsError)
            {
                return result.ToApiResponse();
            }

            return Results.NoContent();
        })
        .WithName("DeleteExampleCategory");
    }
}