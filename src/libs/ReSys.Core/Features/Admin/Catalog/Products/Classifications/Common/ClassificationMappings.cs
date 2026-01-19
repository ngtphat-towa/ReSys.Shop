using Mapster;

using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Core.Features.Admin.Catalog.Products.Classifications.Common;

public class ClassificationMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Classification, ProductClassificationListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TaxonId, src => src.TaxonId)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.TaxonName, src => src.Taxon.Name)
            .Map(dest => dest.TaxonPrettyName, src => src.Taxon.PrettyName);
    }
}
