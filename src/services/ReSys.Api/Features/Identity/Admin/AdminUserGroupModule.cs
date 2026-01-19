using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Identity.Admin.UserGroups.GetGroupsPagedList;
using ReSys.Core.Features.Identity.Admin.UserGroups.CreateGroup;
using ReSys.Core.Features.Identity.Admin.UserGroups.UpdateGroup;
using ReSys.Core.Features.Identity.Admin.UserGroups.DeleteGroup;
using ReSys.Core.Features.Identity.Admin.UserGroups.JoinGroup;
using ReSys.Core.Features.Identity.Admin.UserGroups.LeaveGroup;
using ReSys.Infrastructure.Security.Authorization;
using ReSys.Shared.Constants.Permissions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Identity.Admin;

public sealed class AdminUserGroupModule : ICarterModule
{
    private const string BaseRoute = "api/admin/identity/groups";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - Admin User Groups")
            .RequireAuthorization();

        // 1. Group CRUD
        group.MapGet("/", GetGroupsHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.List);
        group.MapPost("/", CreateGroupHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.View); // Admin.Manage proxy
        group.MapPut("{id:guid}", UpdateGroupHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.View);
        group.MapDelete("{id:guid}", DeleteGroupHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.View);

        // 2. Group Memberships
        group.MapPost("{groupId:guid}/members", JoinGroupHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.AssignRole);
        group.MapDelete("{groupId:guid}/members/{userId}", LeaveGroupHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.UnassignRole);
    }

    private static async Task<IResult> GetGroupsHandler([AsParameters] GetGroupsPagedList.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetGroupsPagedList.Query(options));
        return result.ToTypedApiResponse("User groups retrieved");
    }

    private static async Task<IResult> CreateGroupHandler([FromBody] CreateGroup.Request request, ISender mediator)
    {
        var result = await mediator.Send(new CreateGroup.Command(request));
        return result.ToTypedApiResponse("User group created");
    }

    private static async Task<IResult> UpdateGroupHandler([FromRoute] Guid id, [FromBody] UpdateGroup.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateGroup.Command(id, request));
        return result.ToTypedApiResponse("User group updated");
    }

    private static async Task<IResult> DeleteGroupHandler([FromRoute] Guid id, ISender mediator)
    {
        var result = await mediator.Send(new DeleteGroup.Command(id));
        return result.ToTypedApiResponse("User group deleted");
    }

    private static async Task<IResult> JoinGroupHandler([FromRoute] Guid groupId, [FromBody] JoinGroup.Request request, ISender mediator)
    {
        var result = await mediator.Send(new JoinGroup.Command(groupId, request));
        return result.ToTypedApiResponse("User joined group");
    }

    private static async Task<IResult> LeaveGroupHandler([FromRoute] Guid groupId, [FromRoute] string userId, ISender mediator)
    {
        var result = await mediator.Send(new LeaveGroup.Command(groupId, userId));
        return result.ToTypedApiResponse("User left group");
    }
}