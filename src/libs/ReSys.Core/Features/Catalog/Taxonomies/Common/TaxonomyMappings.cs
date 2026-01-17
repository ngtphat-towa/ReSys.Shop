using Mapster;

using ReSys.Core.Domain.Catalog.Taxonomies;

namespace ReSys.Core.Features.Catalog.Taxonomies.Common;

public class TaxonomyMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Taxonomy, TaxonomyListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.TaxonCount, src => src.Taxons.Count);

        config.NewConfig<Taxonomy, TaxonomyDetail>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Presentation, src => src.Presentation)
            .Map(dest => dest.Position, src => src.Position)
            .Map(dest => dest.PublicMetadata, src => src.PublicMetadata)
            .Map(dest => dest.PrivateMetadata, src => src.PrivateMetadata);

        config.NewConfig<Taxonomy, TaxonomySelectListItem>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<Taxonomy, CreateTaxonomy.CreateTaxonomy.Response>().Inherits<Taxonomy, TaxonomyDetail>();
        config.NewConfig<Taxonomy, UpdateTaxonomy.UpdateTaxonomy.Response>().Inherits<Taxonomy, TaxonomyDetail>();
        config.NewConfig<Taxonomy, GetTaxonomyDetail.GetTaxonomyDetail.Response>().Inherits<Taxonomy, TaxonomyDetail>();
        config.NewConfig<Taxonomy, GetTaxonomiesPagedList.GetTaxonomiesPagedList.Response>().Inherits<Taxonomy, TaxonomyListItem>();
        config.NewConfig<Taxonomy, GetTaxonomySelectList.GetTaxonomySelectList.Response>().Inherits<Taxonomy, TaxonomySelectListItem>();
    }
}