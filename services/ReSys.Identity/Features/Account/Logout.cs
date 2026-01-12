using ErrorOr;


using MediatR;

namespace ReSys.Identity.Features.Account;

public static class Logout
{
    public record Command : IRequest<ErrorOr<Success>>;

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        public Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            // For a headless API using JWT/OIDC, logout is typically handled by the client 
            // by deleting the token. If using server-side blacklisting, it would happen here.
            return Task.FromResult<ErrorOr<Success>>(Result.Success);
        }
    }
}
