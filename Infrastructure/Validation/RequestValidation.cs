using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HouseRentMgmt.Api.Infrastructure.Validation;

public static class RequestPatterns
{
    public const string Username = @"[a-zA-Z0-9\-._@+]+";
    public const string Phone = @"(\+977)?9[6-9][0-9]{8}";
    public const string Email = @"(?!\.)(?!.*\.\.)([A-Za-z0-9_'+\-.]*)[A-Za-z0-9_+-]@([A-Za-z0-9][A-Za-z0-9\-]*\.)+[A-Za-z]{2,}";
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NonDefaultAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value switch
    {
        Guid id => id != Guid.Empty,
        DateOnly date => date != default,
        _ => false
    };
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class RegistrationPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string password || password.Length < 6)
            return ValidationResult.Success; // Required/MinLength handle these cases.

        (string Pattern, string Message)[] rules =
        [
            ("[A-Z]", "Password must contain at least one uppercase letter"),
            ("[a-z]", "Password must contain at least one lowercase letter"),
            ("[0-9]", "Password must contain at least one digit"),
            ("[^a-zA-Z0-9]", "Password must contain at least one special character")
        ];
        foreach (var (pattern, message) in rules)
        {
            if (!Regex.IsMatch(password, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
                return new ValidationResult(message, [context.MemberName!]);
        }
        return ValidationResult.Success;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotFutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not DateOnly date) return ValidationResult.Success;

        if (date > NepalDate.Today)
        {
            return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.", [context.MemberName!]);
        }

        return ValidationResult.Success;
    }
}

// Calendar dates remain Gregorian; only the business day's timezone changes.
public static class NepalDate
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");

    public static DateOnly Today => FromInstant(DateTimeOffset.UtcNow);

    public static DateOnly FromInstant(DateTimeOffset instant) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Zone).DateTime);
}
