using Mapster;
using ReSys.Core.Domain.Identity.UserGroups;

namespace ReSys.Core.Features.Identity.Admin.UserGroups.Common;

public class UserGroupMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserGroup, GroupResponse>()
            .Map(dest => dest.MemberCount, src => src.Memberships.Count);
    }
}