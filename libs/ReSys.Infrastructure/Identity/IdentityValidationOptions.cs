using System.ComponentModel.DataAnnotations;

namespace ReSys.Infrastructure.Identity;

public class IdentityValidationOptions
{
    [Required(ErrorMessage = "Identity Service URL is required.")]
    [Url(ErrorMessage = "Identity Service URL must be a valid URL.")]
    public string Authority { get; set; } = string.Empty;
}
