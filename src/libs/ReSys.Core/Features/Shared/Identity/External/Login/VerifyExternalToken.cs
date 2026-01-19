using MediatR;
using ReSys.Core.Common.Security.Authentication.External;
using ReSys.Core.Common.Security.Authentication.External;
using ErrorOr;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.External.Login;

public static class VerifyExternalToken
{
    public record Request(string Provider, string? AccessToken = null, string? IdToken = null);
    public record Query(Request Request) : IRequest<ErrorOr<ExternalUserTransfer>>;

    public class Handler(IExternalTokenValidator tokenValidator) : IRequestHandler<Query, ErrorOr<ExternalUserTransfer>>
    {
        public async Task<ErrorOr<ExternalUserTransfer>> Handle(Query query, CancellationToken ct)
        {
            var req = query.Request;
            return await tokenValidator.ValidateTokenAsync(
                req.Provider, req.AccessToken, req.IdToken, null, null, ct);
        }
    }
}
