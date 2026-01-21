using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Storefront.Cart;
using ReSys.Core.Features.Storefront.Ordering.Common;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Storefront.Cart;

public class CartEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/storefront/cart")
            .WithTags("Storefront.Cart");

        // Allow Guest or User
        group.MapPost("items", async (AddToCart.Command command, [FromHeader(Name = "X-Session-ID")] string? sessionId, ISender sender) =>
        {
            var cmd = command with { SessionId = sessionId };
            var result = await sender.Send(cmd);
            return result.ToTypedApiResponse("Item added to cart");
        });

        // Allow Guest or User
        group.MapGet("", async ([FromHeader(Name = "X-Session-ID")] string? sessionId, ISender sender) =>
        {
            var result = await sender.Send(new GetCart.Query(sessionId));
            return result.ToTypedApiResponse();
        });

        // Strict Authorization: Must be logged in to merge
        group.MapPost("merge", async (MergeCart.Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToTypedApiResponse("Cart merged successfully");
        }).RequireAuthorization();

        group.MapDelete("promotion", async (ISender sender) =>
        {
            var result = await sender.Send(new RemovePromotion.Command());
            return result.ToTypedApiResponse("Promotion removed");
        }).RequireAuthorization();

        group.MapPost("promotion", async (ApplyPromotionCode.Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToTypedApiResponse("Promotion applied");
        }).RequireAuthorization();
    }
}