using Carter;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using ReSys.Core.Features.Shared.Identity.Admin.Permissions.GetPermissionsPagedList;
using ReSys.Core.Features.Shared.Identity.Admin.Permissions.GetPermissionSelectList;
using ReSys.Infrastructure.Security.Authorization;
using ReSys.Shared.Constants.Permissions;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Identity.Admin;

public sealed class AdminPermissionModule : ICarterModule
{
    private const string BaseRoute = "api/admin/identity/permissions";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - Admin Permissions")
            .RequireAuthorization();

        group.MapGet("/", GetPermissionsHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.List);
        group.MapGet("select", GetPermissionSelectHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.List);
    }

    private static async Task<IResult> GetPermissionsHandler([AsParameters] GetPermissionsPagedList.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetPermissionsPagedList.Query(options));
        return result.ToTypedApiResponse("Permissions list retrieved");
    }

    private static async Task<IResult> GetPermissionSelectHandler([AsParameters] QueryOptions options, ISender mediator)
    {
        var result = await mediator.Send(new GetPermissionSelectList.Query(options));
        return result.ToTypedApiResponse("Permission select items retrieved");
    }
}
