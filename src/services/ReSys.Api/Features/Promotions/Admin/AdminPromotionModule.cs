using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Promotions.Admin.GetPromotionsPagedList;
using ReSys.Core.Features.Promotions.Admin.GetPromotionDetail;
using ReSys.Core.Features.Promotions.Admin.CreatePromotion;
using ReSys.Core.Features.Promotions.Admin.UpdatePromotion;
using ReSys.Core.Features.Promotions.Admin.DeletePromotion;
using ReSys.Core.Features.Promotions.Admin.Rules.AddProductRule;
using ReSys.Core.Features.Promotions.Admin.Rules.AddCategoryRule;
using ReSys.Core.Features.Promotions.Admin.Rules.AddGroupRule;
using ReSys.Core.Features.Promotions.Admin.Actions.UpdateAction;
using ReSys.Core.Features.Promotions.Admin.GetPromotionStats;
using ReSys.Core.Features.Promotions.Admin.GetPromotionUsageHistory;
using ReSys.Core.Features.Promotions.Admin.ActivatePromotion;
using ReSys.Core.Features.Promotions.Admin.DeactivatePromotion;
using ReSys.Core.Features.Promotions.Admin.PreviewPromotion;
using ReSys.Infrastructure.Security.Authorization;
using ReSys.Shared.Constants.Permissions;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Promotions.Admin;

public sealed class AdminPromotionModule : ICarterModule
{
    private const string BaseRoute = "api/admin/promotions";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Promotions - Admin")
            .RequireAuthorization();

        // 1. Basic CRUD
        group.MapGet("/", GetPromotionsHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.View);
        group.MapGet("{id:guid}", GetPromotionDetailHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.View);
        group.MapPost("/", CreatePromotionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Create);
        group.MapPut("{id:guid}", UpdatePromotionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);
        group.MapDelete("{id:guid}", DeletePromotionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Delete);

        // 2. Rules Management
        group.MapPost("{id:guid}/rules/product", AddProductRuleHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);
        group.MapPost("{id:guid}/rules/category", AddCategoryRuleHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);
        group.MapPost("{id:guid}/rules/group", AddGroupRuleHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);

        // 3. Action Management
        group.MapPut("{id:guid}/action", UpdateActionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);

        // 4. Analytics
        group.MapGet("{id:guid}/stats", GetPromotionStatsHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.View);
        group.MapGet("{id:guid}/history", GetPromotionHistoryHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.View);

        // 5. Lifecycle
        group.MapPost("{id:guid}/activate", ActivatePromotionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);
        group.MapPost("{id:guid}/deactivate", DeactivatePromotionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.Edit);

        // 6. Preview
        group.MapPost("{id:guid}/preview", PreviewPromotionHandler).RequireAccessPermission(FeaturePermissions.Admin.Catalog.View);
    }

    private static async Task<IResult> GetPromotionsHandler([AsParameters] GetPromotionsPagedList.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetPromotionsPagedList.Query(options));
        return result.ToTypedApiResponse("Promotions list retrieved");
    }

    private static async Task<IResult> GetPromotionDetailHandler([FromRoute] Guid id, ISender mediator)
    {
        var result = await mediator.Send(new GetPromotionDetail.Query(id));
        return result.ToTypedApiResponse("Promotion details retrieved");
    }

    private static async Task<IResult> CreatePromotionHandler([FromBody] CreatePromotion.Request request, ISender mediator)
    {
        var result = await mediator.Send(new CreatePromotion.Command(request));
        return result.ToTypedApiResponse("Promotion created successfully");
    }

    private static async Task<IResult> UpdatePromotionHandler([FromRoute] Guid id, [FromBody] UpdatePromotion.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdatePromotion.Command(id, request));
        return result.ToTypedApiResponse("Promotion updated successfully");
    }

    private static async Task<IResult> DeletePromotionHandler([FromRoute] Guid id, ISender mediator)
    {
        var result = await mediator.Send(new DeletePromotion.Command(id));
        return result.ToTypedApiResponse("Promotion deleted successfully");
    }

    private static async Task<IResult> AddProductRuleHandler([FromRoute] Guid id, [FromBody] AddProductRule.Request request, ISender mediator)
    {
        var result = await mediator.Send(new AddProductRule.Command(id, request));
        return result.ToTypedApiResponse("Product rule added");
    }

    private static async Task<IResult> AddCategoryRuleHandler([FromRoute] Guid id, [FromBody] AddCategoryRule.Request request, ISender mediator)
    {
        var result = await mediator.Send(new AddCategoryRule.Command(id, request));
        return result.ToTypedApiResponse("Category rule added");
    }

    private static async Task<IResult> AddGroupRuleHandler([FromRoute] Guid id, [FromBody] AddGroupRule.Request request, ISender mediator)
    {
        var result = await mediator.Send(new AddGroupRule.Command(id, request));
        return result.ToTypedApiResponse("Group rule added");
    }

    private static async Task<IResult> UpdateActionHandler(Guid id, [FromBody] UpdateAction.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateAction.Command(id, request));
        return result.ToTypedApiResponse("Promotion action updated");
    }

    private static async Task<IResult> GetPromotionStatsHandler(Guid id, ISender mediator)
    {
        var result = await mediator.Send(new GetPromotionStats.Query(id));
        return result.ToTypedApiResponse("Promotion statistics retrieved");
    }

    private static async Task<IResult> GetPromotionHistoryHandler(Guid id, [AsParameters] QueryOptions options, ISender mediator)
    {
        var result = await mediator.Send(new GetPromotionUsageHistory.Query(id, options));
        return result.ToTypedApiResponse("Promotion usage history retrieved");
    }

    private static async Task<IResult> ActivatePromotionHandler(Guid id, ISender mediator)
    {
        var result = await mediator.Send(new ActivatePromotion.Command(id));
        return result.ToTypedApiResponse("Promotion activated");
    }

    private static async Task<IResult> DeactivatePromotionHandler(Guid id, ISender mediator)
    {
        var result = await mediator.Send(new DeactivatePromotion.Command(id));
        return result.ToTypedApiResponse("Promotion deactivated");
    }

    private static async Task<IResult> PreviewPromotionHandler(Guid id, [FromBody] PreviewPromotion.Request request, ISender mediator)
    {
        var result = await mediator.Send(new PreviewPromotion.Query(id, request));
        return result.ToTypedApiResponse("Promotion preview calculated");
    }
}