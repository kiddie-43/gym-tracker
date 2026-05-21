using GymTracker.Domain.Services;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Domain.Entities;

public sealed class Exercise : BaseEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public string Difficulty { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> MeasurementTypeIds { get; private set; } = Array.Empty<string>();

    public string MeasurementTypeId => MeasurementTypeIds.FirstOrDefault() ?? string.Empty;

    public string MeasurementTypeCode { get; private set; } = string.Empty;

    public string ExerciseTypeId => Category;

    public string ExerciseTypeCode => Difficulty;

    public string FormTypeId => MeasurementTypeId;

    public string FormTypeCode => MeasurementTypeCode;

    public IReadOnlyCollection<string> PrimaryMuscleIds { get; private set; } = Array.Empty<string>();

    public IReadOnlyCollection<string> SecondaryMuscleIds { get; private set; } = Array.Empty<string>();

    public IReadOnlyCollection<string> MuscleGroupIds { get; private set; } = Array.Empty<string>();

    public bool Active { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<ExerciseMedia> Media { get; private set; } = Array.Empty<ExerciseMedia>();

    public static Exercise Create(
        string name,
        string code,
        string? description,
        string category,
        string difficulty,
        IEnumerable<string> measurementTypeIds,
        string measurementTypeCode,
        IEnumerable<string> primaryMuscleIds,
        IEnumerable<string> secondaryMuscleIds,
        IEnumerable<string> muscleGroupIds,
        IEnumerable<ExerciseMedia>? media = null)
    {
        var exercise = new Exercise
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
            Category = ValidateRequired(category, nameof(category)),
            Difficulty = ValidateRequired(difficulty, nameof(difficulty)),
            MeasurementTypeIds = NormalizeIds(measurementTypeIds, nameof(measurementTypeIds)),
            MeasurementTypeCode = ValidateRequired(measurementTypeCode, nameof(measurementTypeCode)),
            PrimaryMuscleIds = NormalizeIds(primaryMuscleIds, nameof(primaryMuscleIds)),
            SecondaryMuscleIds = NormalizeIds(secondaryMuscleIds, nameof(secondaryMuscleIds), allowEmpty: true),
            MuscleGroupIds = NormalizeIds(muscleGroupIds, nameof(muscleGroupIds)),
            Media = media?.ToArray() ?? Array.Empty<ExerciseMedia>(),
        };

        ExerciseMediaRules.ValidateCollection(exercise.Media);
        return exercise;
    }

    public void Update(
        string name,
        string code,
        string? description,
        string category,
        string difficulty,
        IEnumerable<string> measurementTypeIds,
        string measurementTypeCode,
        IEnumerable<string> primaryMuscleIds,
        IEnumerable<string> secondaryMuscleIds,
        IEnumerable<string> muscleGroupIds,
        bool active)
    {
        Name = ValidateName(name);
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
        Category = ValidateRequired(category, nameof(category));
        Difficulty = ValidateRequired(difficulty, nameof(difficulty));
        MeasurementTypeIds = NormalizeIds(measurementTypeIds, nameof(measurementTypeIds));
        MeasurementTypeCode = ValidateRequired(measurementTypeCode, nameof(measurementTypeCode));
        PrimaryMuscleIds = NormalizeIds(primaryMuscleIds, nameof(primaryMuscleIds));
        SecondaryMuscleIds = NormalizeIds(secondaryMuscleIds, nameof(secondaryMuscleIds), allowEmpty: true);
        MuscleGroupIds = NormalizeIds(muscleGroupIds, nameof(muscleGroupIds));
        Active = active;
        Touch();
    }

    public void ReplaceMedia(IReadOnlyCollection<ExerciseMedia> media)
    {
        Media = media;
        ExerciseMediaRules.ValidateCollection(Media);
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

    public string? ResolveCoverStoragePath()
    {
        var activeItems = Media.Where(item => item.Active && !item.IsDeleted).ToArray();
        var primary = activeItems.FirstOrDefault(item => item.IsPrimary);
        return (primary ?? activeItems.OrderBy(item => item.SortOrder).FirstOrDefault())?.StoragePath;
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

    private static string ValidateRequired(string value, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", argumentName);
        }

        return value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeIds(IEnumerable<string> values, string argumentName, bool allowEmpty = false)
    {
        var normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!allowEmpty && normalized.Length == 0)
        {
            throw new ArgumentException("At least one id is required.", argumentName);
        }

        return normalized;
    }
}
