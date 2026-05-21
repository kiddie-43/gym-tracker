namespace GymTracker.Domain.Entities;

public sealed class ExerciseType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? PrimaryUnit { get; private set; }

    public bool RequiresUnits { get; private set; }

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public static ExerciseType Create(string name, string code, string? description, string? primaryUnit, bool requiresUnits)
    {
        var normalizedUnit = NormalizeUnit(primaryUnit, requiresUnits);

        return new ExerciseType
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
            PrimaryUnit = normalizedUnit,
            RequiresUnits = requiresUnits,
        };
    }

    public void Update(string name, string code, string? description, string? primaryUnit, bool requiresUnits, bool active)
    {
        Name = ValidateName(name);
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
        PrimaryUnit = NormalizeUnit(primaryUnit, requiresUnits);
        RequiresUnits = requiresUnits;
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

    private static string? NormalizeUnit(string? value, bool requiresUnits)
    {
        if (requiresUnits && string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Primary unit is required when units are required.", nameof(value));
        }

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
