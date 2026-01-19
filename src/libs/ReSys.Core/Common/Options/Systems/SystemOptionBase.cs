using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace ReSys.Core.Common.Options.Systems;

/// <summary>
/// Base class for shared system configuration options.
/// </summary>
public abstract class SystemOptionBase
{
    [Required, MinLength(3)]
    public string SystemName { get; set; } = string.Empty;

    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string SupportEmail { get; set; } = string.Empty;

    [Required]
    public string DefaultPage { get; set; } = string.Empty;
}

/// <summary>
/// Generic validator for <see cref="SystemOptionBase"/> implementations.
/// </summary>
public sealed class SystemOptionValidator<TOption> : IValidateOptions<TOption>
    where TOption : SystemOptionBase
{
    public ValidateOptionsResult Validate(string? name, TOption? options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("Options instance cannot be null.");

        List<ValidationResult> results = [];
        ValidationContext context = new(options);

        if (!Validator.TryValidateObject(options, context, results, true))
        {
            return ValidateOptionsResult.Fail(results.Select(r => r.ErrorMessage ?? "Unknown validation error"));
        }

        return ValidateOptionsResult.Success;
    }
}