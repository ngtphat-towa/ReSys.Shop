using System.ComponentModel.DataAnnotations;

namespace ReSys.Infrastructure.ML;

public class MlOptions
{
    public const string SectionName = "ML";

    [Required, Url]
    public string ServiceUrl { get; set; } = string.Empty;
}
