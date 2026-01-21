using Mapster;

using ReSys.Core.Domain.Promotions;

namespace ReSys.Core.Features.Admin.Promotions.Common;

public class PromotionAdminMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Promotion, PromotionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.MinimumOrderAmount, src => src.MinimumOrderAmount)
            .Map(dest => dest.MaximumDiscountAmount, src => src.MaximumDiscountAmount)
            .Map(dest => dest.StartsAt, src => src.StartsAt)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt)
            .Map(dest => dest.UsageLimit, src => src.UsageLimit)
            .Map(dest => dest.RequiresCouponCode, src => src.RequiresCouponCode)
            .Map(dest => dest.UsageCount, src => src.UsageCount)
            .Map(dest => dest.Active, src => src.Active)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.IsExpired, src => src.IsExpired);
    }
}
