namespace GymTracker.Application.Admin.Common;

public static class AdminUniqueCodeValidator
{
    public static string Normalize(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    public static bool Exists(string candidateCode, IEnumerable<string> existingCodes)
    {
        var normalizedCandidate = Normalize(candidateCode);

        foreach (var existingCode in existingCodes)
        {
            if (string.IsNullOrWhiteSpace(existingCode))
            {
                continue;
            }

            if (Normalize(existingCode) == normalizedCandidate)
            {
                return true;
            }
        }

        return false;
    }

    public static void EnsureUnique(string candidateCode, IEnumerable<string> existingCodes, string? argumentName = null)
    {
        if (Exists(candidateCode, existingCodes))
        {
              throw new InvalidOperationException("Duplicate code is not allowed.");
        }
    }
}
