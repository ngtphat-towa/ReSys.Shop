using ErrorOr;

namespace ReSys.Core.Domain.Common.Abstractions;

public static class CommonErrors
{
    public static class Validation
    {
        public static Error Required(string field) => Error.Validation(
            code: $"{field}.Required",
            description: $"{field} is required.");

        public static Error TooLong(string field, int maxLength) => Error.Validation(
            code: $"{field}.TooLong",
            description: $"{field} must not exceed {maxLength} characters.");

        public static Error InvalidFormat(string field) => Error.Validation(
            code: $"{field}.InvalidFormat",
            description: $"{field} has an invalid format.");
            
        public static Error NegativeValue(string field) => Error.Validation(
            code: $"{field}.NegativeValue",
            description: $"{field} cannot be negative.");
    }
}