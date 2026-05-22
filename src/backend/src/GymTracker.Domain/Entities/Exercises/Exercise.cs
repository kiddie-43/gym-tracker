namespace GymTracker.Domain.Entities;

public sealed class Exercise : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Code { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public string Difficulty { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<string> PrimaryMuscleIds { get; private set; } = [];

    public IReadOnlyCollection<string> SecondaryMuscleIds { get; private set; } = [];

    public IReadOnlyCollection<string> MeasurementTypeIds { get; private set; } = [];

    public static Exercise Create(
        string name,
        string? code,
        string category,
        string difficulty,
        IEnumerable<string> primaryMuscleIds,
        IEnumerable<string> secondaryMuscleIds,
        IEnumerable<string> measurementTypeIds)
    {
        return new Exercise
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Category = ValidateCategory(category),
            Difficulty = ValidateDifficulty(difficulty),
            PrimaryMuscleIds = NormalizeRelationIds(primaryMuscleIds, nameof(primaryMuscleIds)),
            SecondaryMuscleIds = NormalizeRelationIds(secondaryMuscleIds, nameof(secondaryMuscleIds), requireAtLeastOne: false),
            MeasurementTypeIds = NormalizeRelationIds(measurementTypeIds, nameof(measurementTypeIds)),
        };
    }

    public void Update(
        string name,
        string? code,
        string category,
        string difficulty,
        IEnumerable<string> primaryMuscleIds,
        IEnumerable<string> secondaryMuscleIds,
        IEnumerable<string> measurementTypeIds,
        bool active)
    {
        Name = ValidateName(name);
        Code = NormalizeCode(code);
        Category = ValidateCategory(category);
        Difficulty = ValidateDifficulty(difficulty);
        PrimaryMuscleIds = NormalizeRelationIds(primaryMuscleIds, nameof(primaryMuscleIds));
        SecondaryMuscleIds = NormalizeRelationIds(secondaryMuscleIds, nameof(secondaryMuscleIds), requireAtLeastOne: false);
        MeasurementTypeIds = NormalizeRelationIds(measurementTypeIds, nameof(measurementTypeIds));
        Active = active;
        Touch();
    }

    public void LoadRelations(
        IEnumerable<string>? primaryMuscleIds,
        IEnumerable<string>? secondaryMuscleIds,
        IEnumerable<string>? measurementTypeIds)
    {
        PrimaryMuscleIds = NormalizeRelationIds(primaryMuscleIds, nameof(primaryMuscleIds), requireAtLeastOne: false);
        SecondaryMuscleIds = NormalizeRelationIds(secondaryMuscleIds, nameof(secondaryMuscleIds), requireAtLeastOne: false);
        MeasurementTypeIds = NormalizeRelationIds(measurementTypeIds, nameof(measurementTypeIds), requireAtLeastOne: false);
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
            throw new ArgumentException("Name is required.", nameof(value));
        return value.Trim();
    }

    private static string? NormalizeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }

    private static string ValidateCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Category is required.", nameof(value));
        return value.Trim();
    }

    private static string ValidateDifficulty(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Difficulty is required.", nameof(value));
        return value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeRelationIds(
        IEnumerable<string>? ids,
        string parameterName,
        bool requireAtLeastOne = true)
    {
        var normalized = (ids ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (requireAtLeastOne && normalized.Length == 0)
            throw new ArgumentException("At least one related id is required.", parameterName);

        return normalized;
    }
}

