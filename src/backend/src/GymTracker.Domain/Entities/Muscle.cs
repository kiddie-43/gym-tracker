namespace GymTracker.Domain.Entities;

public sealed class Muscle : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IReadOnlyCollection<string> MuscleGroupIds { get; private set; } = Array.Empty<string>();

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public static Muscle Create(string name, string code, string? description, IEnumerable<string> muscleGroupIds)
    {
        return new Muscle
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
            MuscleGroupIds = NormalizeMuscleGroupIds(muscleGroupIds),
        };
    }

    public void Update(string name, string code, string? description, IEnumerable<string> muscleGroupIds, bool active)
    {
        Name = ValidateName(name);
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
        MuscleGroupIds = NormalizeMuscleGroupIds(muscleGroupIds);
        Active = active;
        Touch();
    }

    public void SoftDelete(DateTimeOffset now)
    {
        IsDeleted = true;
        Active = false;
        DeletedAt = now;
        Touch();
    }

    public void Reactivate()
    {
        IsDeleted = false;
        Active = true;
        DeletedAt = null;
        Touch();
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
