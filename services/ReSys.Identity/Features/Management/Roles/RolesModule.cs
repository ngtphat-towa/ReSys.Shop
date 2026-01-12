using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Common.Constants;
using ReSys.Identity.Presentation.Extensions;
using ReSys.Infrastructure.Authentication.Authorization;

namespace ReSys.Identity.Features.Management.Roles;

public class RolesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/management/roles")
            .WithTags("Management - Roles");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new List.Query(), ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok(result.Value);
        })
        .RequirePermission(AppPermissions.Identity.Roles.List);

        group.MapPost("/", async ([FromBody] Create.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new Create.Command(request);
            var result = await sender.Send(command, ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequirePermission(AppPermissions.Identity.Roles.Create);

        group.MapPut("/{roleId}/permissions", async (string roleId, [FromBody] UpdatePermissions.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdatePermissions.Command(roleId, request);
            var result = await sender.Send(command, ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .WithName("UpdateRolePermissions")
        .RequirePermission(AppPermissions.Identity.Roles.ManagePermissions);
    }
}
