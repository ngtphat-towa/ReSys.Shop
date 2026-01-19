using Mapster;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Tokens;
using ReSys.Core.Common.Security.Authentication.Tokens.Models;

namespace ReSys.Core.Features.Shared.Identity.Internal.Common;

public class InternalIdentityMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 1. User -> Profile Response
        config.NewConfig<User, UserProfileResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.IsEmailConfirmed, src => src.EmailConfirmed)
            .Map(dest => dest.LastSignInAt, src => src.LastSignInAt);

        // 2. Token -> Session Response
        config.NewConfig<RefreshToken, ActiveSessionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CreatedByIp, src => src.CreatedByIp)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt);

        // 3. Internal Token Result -> API Response
        config.NewConfig<AuthenticationResult, AuthenticationResponse>();
    }
}
