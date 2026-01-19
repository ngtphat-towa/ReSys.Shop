using Mapster;
using ReSys.Core.Domain.Identity.Roles;

namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.Common;

public class RoleAdminMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Role, RoleResponse>()
            .Map(dest => dest.UserCount, src => src.UserRoles.Count);
    }
}
