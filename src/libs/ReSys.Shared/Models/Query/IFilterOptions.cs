namespace ReSys.Shared.Models;

public interface IFilterOptions
{
    string? Filter { get; }
}

public record FilterOptions : IFilterOptions
{
    public string? Filter { get; set; }
}
