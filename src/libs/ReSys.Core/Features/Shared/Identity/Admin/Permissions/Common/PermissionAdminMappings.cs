using Mapster;

using ReSys.Core.Domain.Identity.Permissions;

namespace ReSys.Core.Features.Shared.Identity.Admin.Permissions.Common;

public class PermissionAdminMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccessPermission, PermissionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.DisplayName, src => src.DisplayName)
            .Map(dest => dest.Description, src => src.Description);

    }
}
