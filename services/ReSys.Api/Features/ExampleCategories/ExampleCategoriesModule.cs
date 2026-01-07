using Carter;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;
using ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategories;
using ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategoryById;
using ReSys.Core.Features.Testing.ExampleCategories.UpdateExampleCategory;
using ReSys.Core.Features.Testing.ExampleCategories.DeleteExampleCategory;
using ReSys.Core.Common.Models;
using ReSys.Api.Infrastructure.Extensions;

namespace ReSys.Api.Features.ExampleCategories;

public class ExampleCategoriesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/testing/example-categories")
            .WithTags("Example Categories");

        group.MapGet("/", async ([AsParameters] GetExampleCategories.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetExampleCategories.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetExampleCategories");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExampleCategoryById.Query(id));
            return result.ToApiResponse();
        })
        .WithName("GetExampleCategoryById");

        group.MapPost("/", async (
            [FromBody] CreateExampleCategory.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateExampleCategory.Command(request), ct);
            return result.ToApiCreatedResponse(category => $"/api/testing/example-categories/{category.Id}");
        })
        .WithName("CreateExampleCategory");

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
