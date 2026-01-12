using Carter;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ReSys.Core.Features.Testing.Examples.CreateExample;
using ReSys.Core.Features.Testing.Examples.GetExamples;
using ReSys.Core.Features.Testing.Examples.GetExampleById;
using ReSys.Core.Features.Testing.Examples.UpdateExample;
using ReSys.Core.Features.Testing.Examples.UpdateExampleImage;
using ReSys.Core.Features.Testing.Examples.DeleteExample;
using ReSys.Core.Common.Constants;
using ReSys.Core.Common.Models;
using ReSys.Api.Infrastructure.Extensions;

namespace ReSys.Api.Features.Examples;

public class ExamplesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/testing/examples")
            .WithTags("Examples");

        group.MapGet("/", async ([AsParameters] GetExamples.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetExamples.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetExamples");

        group.MapGet("/v2", async ([AsParameters] GetExamplesV2.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetExamplesV2.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetExamplesV2");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExampleById.Query(new GetExampleById.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetExampleById");

        group.MapPost("/", async (
            [FromBody] CreateExample.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateExample.Command(request), ct);
            return result.ToApiCreatedResponse(example => $"/api/testing/examples/{example.Id}");
        })
        .WithName("CreateExample")
        .RequireAuthorization(p => p.RequireRole(AuthConstants.Roles.Admin));

        group.MapPut("/{id}", async (
            Guid id,
            [FromBody] UpdateExample.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateExample.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateExample")
        .RequireAuthorization(p => p.RequireRole(AuthConstants.Roles.Admin));

        group.MapPost("/{id}/image", async (
            Guid id,
            IFormFile image,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateExampleImage.Command(new UpdateExampleImage.Request(id, image.OpenReadStream(), image.FileName)), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateExampleImage")
        .DisableAntiforgery()
        .RequireAuthorization(p => p.RequireRole(AuthConstants.Roles.Admin));

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteExample.Command(id));

            if (result.IsError)
            {
                return result.ToApiResponse();
            }

            return Results.NoContent();
        })
        .WithName("DeleteExample")
        .RequireAuthorization(p => p.RequireRole(AuthConstants.Roles.Admin));
    }
}