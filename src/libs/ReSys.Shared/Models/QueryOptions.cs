namespace ReSys.Shared.Models;

// 1. Filtering
public record FilterOptions : IFilterOptions
{
    public string? Filter { get; set; }
}

// 2. Sorting
public record SortOptions : ISortOptions
{
    public string? Sort { get; set; }
}

// 3. Searching
public record SearchOptions : ISearchOptions
{
    public string? Search { get; set; }
    public string[]? SearchField { get; set; }
}

// 4. Pagination
public record PageOptions : IPageOptions
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

// 5. Unified (Composition/Inheritance)
public record QueryOptions : IFilterOptions, ISortOptions, ISearchOptions, IPageOptions
{
    public string? Filter { get; set; }
    public string? Sort { get; set; }
    public string? Search { get; set; }
    public string[]? SearchField { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

// Interfaces
public interface IFilterOptions { string? Filter { get; } }
public interface ISortOptions { string? Sort { get; } }
public interface ISearchOptions { string? Search { get; } string[]? SearchField { get; } }
public interface IPageOptions { int? Page { get; } int? PageSize { get; } }
