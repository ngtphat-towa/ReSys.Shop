namespace ReSys.Core.Domain.Common.Abstractions;

public enum DisplayOn
{
    None,
    Both,
    Storefront,
    BackEnd
}

public interface IHasDisplayOn
{
    DisplayOn DisplayOn { get; }
}
