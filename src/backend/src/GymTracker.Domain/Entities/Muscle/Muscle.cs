namespace GymTracker.Domain.Entities;
using GymTracker.Domain.Common;
 
public sealed class Muscle : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public static Muscle Create(string name, string code, string? description)
    {
        return new Muscle
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
        };
    }

    public void Update(string name, string? description)
    {
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
    }




    private static string ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name is required.", nameof(value));
        }

        return value.Trim();
    }

    private static string NormalizeCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Code is required.", nameof(value));
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeDescription(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeMuscleGroupIds(IEnumerable<string> value)
    {
        var ids = value
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ids.Length == 0)
        {
            throw new ArgumentException("At least one muscle group is required.", nameof(value));
        }

        return ids;
    }
}
