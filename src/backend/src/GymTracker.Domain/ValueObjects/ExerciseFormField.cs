namespace GymTracker.Domain.ValueObjects;

public sealed record ExerciseFormField(
    string Id,
    string Name,
    string Label,
    string Type,
    bool Required,
    string? Unit,
    decimal? Min,
    decimal? Max,
    IReadOnlyCollection<string>? Options,
    int SortOrder)
{
    public static ExerciseFormField Create(
        string id,
        string name,
        string label,
        string type,
        bool required,
        string? unit,
        decimal? min,
        decimal? max,
        IReadOnlyCollection<string>? options,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Field id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Field name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Field label is required.", nameof(label));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Field type is required.", nameof(type));
        }

        if (min.HasValue && max.HasValue && min.Value > max.Value)
        {
            throw new ArgumentException("Field min cannot be greater than max.", nameof(min));
        }

        var normalizedType = type.Trim().ToLowerInvariant();
        if (normalizedType == "select")
        {
            if (options is null || options.Count == 0)
            {
                throw new ArgumentException("Select fields require at least one option.", nameof(options));
            }
        }

        var normalizedOptions = options?
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ExerciseFormField(
            Id: id.Trim(),
            Name: name.Trim(),
            Label: label.Trim(),
            Type: normalizedType,
            Required: required,
            Unit: string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            Min: min,
            Max: max,
            Options: normalizedOptions,
            SortOrder: sortOrder);
    }
}
