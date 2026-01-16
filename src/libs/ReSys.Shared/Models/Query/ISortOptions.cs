namespace ReSys.Shared.Models;

public interface ISortOptions
{
    string? Sort { get; }
}

public record SortOptions : ISortOptions
{
    public string? Sort { get; set; }
}
