using ReSys.Shared.Models.Filters;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Search;

namespace ReSys.Shared.Models.Query;

public record QueryOptions : IFilterOptions, ISortOptions, ISearchOptions, IPageOptions
{
    public string? Filter { get; set; }
    public string? Sort { get; set; }
    public string? Search { get; set; }
    public string[]? SearchField { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
