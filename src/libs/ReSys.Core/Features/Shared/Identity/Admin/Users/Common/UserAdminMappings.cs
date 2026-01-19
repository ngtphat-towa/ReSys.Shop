using Mapster;
using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.Common;

public class UserAdminMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, AdminUserListItem>();
        config.NewConfig<User, AdminUserDetailResponse>();
    }
}
