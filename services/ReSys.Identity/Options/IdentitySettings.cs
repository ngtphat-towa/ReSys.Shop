using System.ComponentModel.DataAnnotations;

namespace ReSys.Identity.Options;

public class IdentitySettings
{
    public const string SectionName = "Identity";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    public bool UseStandardJwt { get; set; } = true;
}
