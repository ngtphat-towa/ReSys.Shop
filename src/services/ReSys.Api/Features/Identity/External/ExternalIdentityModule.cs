using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReSys.Core.Features.Shared.Identity.External.Login;
using ReSys.Core.Features.Shared.Identity.External.Config;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Identity.External;

public sealed class ExternalIdentityModule : ICarterModule
{
    private const string BaseRoute = "api/auth/external";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - External")
            .DisableAntiforgery();

        group.MapGet("providers", GetProvidersHandler);
        group.MapGet("{provider}/config", GetConfigHandler);
        group.MapPost("{provider}/exchange", ExchangeTokenHandler);
        group.MapPost("{provider}/verify", VerifyTokenHandler);
    }

    private static async Task<IResult> GetProvidersHandler(ISender mediator)
    {
        var result = await mediator.Send(new GetExternalProviders.Query());
        return result.ToTypedApiResponse("Providers retrieved successfully");
    }

    private static async Task<IResult> GetConfigHandler([FromRoute] string provider, ISender mediator)
    {
        var result = await mediator.Send(new GetOAuthConfig.Query(new GetOAuthConfig.Request(provider)));
        return result.ToTypedApiResponse($"Configuration for {provider} retrieved");
    }

    private static async Task<IResult> ExchangeTokenHandler(
        [FromRoute] string provider, 
        [FromBody] ExchangeToken.Request request, 
        ISender mediator, 
        HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var result = await mediator.Send(new ExchangeToken.Command(request with { Provider = provider, IpAddress = ip }));
        return result.ToTypedApiResponse("Social login successful");
    }

    private static async Task<IResult> VerifyTokenHandler(
        [FromRoute] string provider, 
        [FromBody] VerifyExternalToken.Request request, 
        ISender mediator)
    {
        var result = await mediator.Send(new VerifyExternalToken.Query(request with { Provider = provider }));
        return result.ToTypedApiResponse("Token verified");
    }
}
