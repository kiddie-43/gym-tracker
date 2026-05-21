namespace GymTracker.Application.Admin.Common;

public static class AdminRequestSanitizer
{
    public static string RequiredTrimmed(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    public static string RequiredUpperCode(string? value, string parameterName)
    {
        return RequiredTrimmed(value, parameterName).ToUpperInvariant();
    }

    public static string? OptionalTrimmed(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
