using Mapster;
using ReSys.Core.Domain.Promotions;

namespace ReSys.Core.Features.Promotions.Admin.Common;

public class PromotionAdminMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Promotion, PromotionResponse>()
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.IsExpired, src => src.IsExpired);
    }
}