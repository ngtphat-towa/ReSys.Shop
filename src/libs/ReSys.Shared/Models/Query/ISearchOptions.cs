namespace ReSys.Shared.Models;

public interface ISearchOptions
{
    string? Search { get; }
    string[]? SearchField { get; }
}

public record SearchOptions : ISearchOptions
{
    public string? Search { get; set; }
    public string[]? SearchField { get; set; }
}
