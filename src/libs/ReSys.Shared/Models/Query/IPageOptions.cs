namespace ReSys.Shared.Models;

public interface IPageOptions
{
    int? Page { get; }
    int? PageSize { get; }
}

public record PageOptions : IPageOptions
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
