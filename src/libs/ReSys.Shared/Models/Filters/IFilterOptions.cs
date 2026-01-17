namespace ReSys.Shared.Models.Filters;

public interface IFilterOptions
{
    string? Filter { get; }
}

public record FilterOptions : IFilterOptions
{
    public string? Filter { get; set; }
}
