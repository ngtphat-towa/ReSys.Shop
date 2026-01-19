using Mapster;
using ReSys.Core.Domain.Identity.Permissions;

namespace ReSys.Core.Features.Shared.Identity.Admin.Permissions.Common;

public class PermissionAdminMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccessPermission, PermissionResponse>();
    }
}
