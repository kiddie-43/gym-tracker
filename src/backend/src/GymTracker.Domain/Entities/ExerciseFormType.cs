using GymTracker.Domain.ValueObjects;

namespace GymTracker.Domain.Entities;

public sealed class ExerciseFormType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IReadOnlyCollection<ExerciseFormField> Fields { get; private set; } = Array.Empty<ExerciseFormField>();

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public static ExerciseFormType Create(string name, string code, string? description, IEnumerable<ExerciseFormField> fields)
    {
        return new ExerciseFormType
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
            Fields = NormalizeFields(fields),
        };
    }

    public void Update(string name, string code, string? description, IEnumerable<ExerciseFormField> fields, bool active)
    {
        Name = ValidateName(name);
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
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

    private static IReadOnlyCollection<ExerciseFormField> NormalizeFields(IEnumerable<ExerciseFormField> value)
    {
        var fields = value.ToArray();
        if (fields.Length == 0)
        {
            throw new ArgumentException("At least one form field is required.", nameof(value));
        }

        var duplicatedNames = fields
            .GroupBy(field => field.Name, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (duplicatedNames)
        {
            throw new ArgumentException("Form fields cannot contain duplicate names.", nameof(value));
        }

        return fields
            .OrderBy(field => field.SortOrder)
            .ToArray();
    }
}
