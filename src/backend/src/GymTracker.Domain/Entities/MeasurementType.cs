namespace GymTracker.Domain.Entities;

public sealed class MeasurementType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> Fields { get; private set; } = Array.Empty<string>();

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public static MeasurementType Create(string name, IEnumerable<string> fields)
    {
        var normalizedName = NormalizeName(name);
        var normalizedFields = NormalizeFields(fields);

        return new MeasurementType
        {
            Name = normalizedName,
            Fields = normalizedFields,
        };
    }

    public void Update(string name, IEnumerable<string> fields, bool active = true)
    {
        Name = NormalizeName(name);
        Fields = NormalizeFields(fields);
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

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name is required.", nameof(value));
        }

        return value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeFields(IEnumerable<string> fields)
    {
        var normalized = fields
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Select(field => field.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one field is required.", nameof(fields));
        }

        return normalized;
    }
}
