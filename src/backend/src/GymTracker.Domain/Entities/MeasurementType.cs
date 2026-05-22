namespace GymTracker.Domain.Entities;

public sealed class MeasurementType : BaseEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public static MeasurementType Create(string code, string name, string? description)
    {
        return new MeasurementType
        {
            Code = NormalizeCode(code),
            Name = NormalizeName(name, code),
            Description = NormalizeDescription(description),
        };
    }

    public void Update(string code, string name, string? description, bool active = true)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name, code);
        Description = NormalizeDescription(description);
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

    private static string NormalizeCode(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Code is required.", nameof(value));
        return normalized;
    }

    private static string NormalizeName(string? name, string? code)
    {
        var trimmed = (name ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(trimmed)
            ? (code ?? string.Empty).Trim().ToUpperInvariant()
            : trimmed;
    }

    private static string? NormalizeDescription(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

