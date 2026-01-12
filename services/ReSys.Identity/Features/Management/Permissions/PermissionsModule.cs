using Carter;


using MediatR;


using Microsoft.AspNetCore.Mvc;


using ReSys.Core.Common.Constants;
using ReSys.Identity.Presentation.Extensions;
using ReSys.Infrastructure.Authentication.Authorization;

namespace ReSys.Identity.Features.Management.Permissions;

public class PermissionsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/management/permissions")
            .WithTags("Management - Permissions");

        group.MapPost("/", async ([FromBody] Create.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new Create.Command(request);
            var result = await sender.Send(command, ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequirePermission(AppPermissions.Identity.PermissionsManagement.Create);

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new List.Query(), ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok(result.Value);
        })
        .RequirePermission(AppPermissions.Identity.PermissionsManagement.List);
    }
}
