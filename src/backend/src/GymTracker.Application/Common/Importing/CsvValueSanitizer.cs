namespace GymTracker.Application.Common.Importing;

public static class CsvValueSanitizer
{
    private const int MaxCellLength = 500;

    public static Dictionary<string, string?> CleanRow(Dictionary<string, string?> row)
    {
        return row.ToDictionary(
            x => x.Key.Trim().ToLowerInvariant(),
            x => Clean(x.Value),
            StringComparer.OrdinalIgnoreCase);
    }

    public static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleaned = value
            .Trim()
            .Replace("\0", string.Empty)
            .Replace("\r", " ")
            .Replace("\n", " ");

        if (cleaned.Length > MaxCellLength)
            cleaned = cleaned[..MaxCellLength];

        return cleaned;
    }

    public static bool IsSafe(string value)
    {
        return value.All(c =>
            char.IsLetterOrDigit(c) ||
            c is '-' or '_' or ' ' or '.' or '/');
    }
}