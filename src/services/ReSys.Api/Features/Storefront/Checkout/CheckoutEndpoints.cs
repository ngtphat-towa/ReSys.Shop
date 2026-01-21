using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Storefront.Checkout;
using ReSys.Core.Features.Storefront.Ordering.Common;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Storefront.Checkout;

public class CheckoutEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/storefront/checkout")
            .WithTags("Storefront.Checkout")
            .RequireAuthorization();

        group.MapPost("addresses", async (SetCheckoutAddressesRequest request, ISender sender) =>
        {
            var result = await sender.Send(new SetCheckoutAddresses.Command(request));
            return result.ToTypedApiResponse("Addresses updated");
        });

        group.MapGet("shipping-methods", async (ISender sender) =>
        {
            var result = await sender.Send(new GetShippingMethods.Query());
            return result.ToTypedApiResponse();
        });

        group.MapPost("shipping-method", async (SetShippingMethodRequest request, ISender sender) =>
        {
            var result = await sender.Send(new SetShippingMethod.Command(request));
            return result.ToTypedApiResponse("Shipping method selected");
        });

        group.MapPost("place-order", async (PlaceOrderRequest request, ISender sender) =>
        {
            var result = await sender.Send(new PlaceOrder.Command(request));
            return result.ToTypedApiResponse("Order placed");
        });
    }
}
