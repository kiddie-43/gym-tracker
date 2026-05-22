using System.Globalization;
using System.Text;

using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Exercises;

public sealed class ExerciseService
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "code", "category", "difficulty",
    };

    private static readonly HashSet<string> AllowedSortDirection = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc", "desc",
    };

    private readonly IExerciseRepository _repository;
    private readonly IMuscleRepository _muscleRepository;
    private readonly IMeasurementTypeRepository _measurementTypeRepository;

    public ExerciseService(
        IExerciseRepository repository,
        IMuscleRepository muscleRepository,
        IMeasurementTypeRepository measurementTypeRepository)
    {
        _repository = repository;
        _muscleRepository = muscleRepository;
        _measurementTypeRepository = measurementTypeRepository;
    }

    public async Task<ExercisesPageResponse> ListPageAsync(
        bool includeDeleted = false,
        string? search = null,
        string? category = null,
        string? difficulty = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedPage = page < 0 ? 0 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var entities = await _repository.ListAsync(includeDeleted: true, cancellationToken);
        var filtered = entities.Where(e => includeDeleted || !e.IsDeleted).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = NormalizeForSearch(search);
            filtered = filtered.Where(e =>
                NormalizeForSearch(e.Name).Contains(s) ||
                NormalizeForSearch(e.Code ?? string.Empty).Contains(s) ||
                NormalizeForSearch(e.Category).Contains(s)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var c = NormalizeForSearch(category);
            filtered = filtered.Where(e => NormalizeForSearch(e.Category).Contains(c)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            var d = NormalizeForSearch(difficulty);
            filtered = filtered.Where(e => NormalizeForSearch(e.Difficulty).Contains(d)).ToList();
        }

        var comparer = StringComparer.OrdinalIgnoreCase;
        IOrderedEnumerable<Exercise> ordered = normalizedSortBy switch
        {
            "code" => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Code ?? string.Empty, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Code ?? string.Empty, comparer).ThenBy(e => e.Id, comparer),
            "category" => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Category, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Category, comparer).ThenBy(e => e.Id, comparer),
            "difficulty" => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Difficulty, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Difficulty, comparer).ThenBy(e => e.Id, comparer),
            _ => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Name, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Name, comparer).ThenBy(e => e.Id, comparer),
        };

        var total = filtered.Count;
        var pagedEntities = ordered
            .Skip(normalizedPage * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToArray();

        var muscleMap = await BuildMuscleMapAsync(cancellationToken);
        var measurementTypeMap = await BuildMeasurementTypeMapAsync(cancellationToken);
        var pagedItems = pagedEntities
            .Select(entity => Map(entity, muscleMap, measurementTypeMap))
            .ToArray();

        return new ExercisesPageResponse(pagedItems, total, normalizedPage, normalizedPageSize);
    }

    public async Task<ExerciseDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return null;

        var muscleMap = await BuildMuscleMapAsync(cancellationToken);
        var measurementTypeMap = await BuildMeasurementTypeMapAsync(cancellationToken);
        return Map(entity, muscleMap, measurementTypeMap);
    }

    public async Task<ExerciseDto> CreateAsync(UpsertExerciseRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            if (await _repository.ExistsActiveCodeAsync(request.Code, cancellationToken: cancellationToken))
                throw new InvalidOperationException("Code already exists among active records.");
        }

        var entity = Exercise.Create(
            request.Name, request.Code, request.Category, request.Difficulty,
            request.PrimaryMuscleIds, request.SecondaryMuscleIds, request.MeasurementTypeIds);

        await _repository.SaveAsync(entity, cancellationToken);
        var muscleMap = await BuildMuscleMapAsync(cancellationToken);
        var measurementTypeMap = await BuildMeasurementTypeMapAsync(cancellationToken);
        return Map(entity, muscleMap, measurementTypeMap);
    }

    public async Task<ExerciseDto?> UpdateAsync(string id, UpsertExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted) return null;

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            if (await _repository.ExistsActiveCodeAsync(request.Code, id, cancellationToken))
                throw new InvalidOperationException("Code already exists among active records.");
        }

        entity.Update(
            request.Name, request.Code, request.Category, request.Difficulty,
            request.PrimaryMuscleIds, request.SecondaryMuscleIds, request.MeasurementTypeIds,
            request.Active);

        await _repository.SaveAsync(entity, cancellationToken);
        var muscleMap = await BuildMuscleMapAsync(cancellationToken);
        var measurementTypeMap = await BuildMeasurementTypeMapAsync(cancellationToken);
        return Map(entity, muscleMap, measurementTypeMap);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);

    public async Task<ExerciseDto?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _repository.ReactivateAsync(id, cancellationToken);
        if (!reactivated) return null;
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        var muscleMap = await BuildMuscleMapAsync(cancellationToken);
        var measurementTypeMap = await BuildMeasurementTypeMapAsync(cancellationToken);
        return Map(entity, muscleMap, measurementTypeMap);
    }

    public async Task<IReadOnlyCollection<ExerciseSearchItem>> SearchAsync(
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(includeDeleted: false, cancellationToken);
        IEnumerable<Exercise> filtered = entities.Where(e => !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var n = NormalizeForSearch(q);
            filtered = filtered.Where(e =>
                NormalizeForSearch(e.Name).Contains(n) ||
                NormalizeForSearch(e.Code ?? string.Empty).Contains(n));
        }

        return filtered
            .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .Select(e => new ExerciseSearchItem(e.Id, e.Name, e.Code, e.Category, e.PrimaryMuscleIds))
            .ToArray();
    }

    // Usado por WorkoutCatalogService
    public async Task<IReadOnlyCollection<ExerciseCatalogItem>> ListWorkoutCatalogAsync(
        string? query,
        IReadOnlyCollection<string>? muscleGroupIds,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAsync(includeDeleted: false, cancellationToken);
        IEnumerable<Exercise> filtered = entities.Where(e => !e.IsDeleted && e.Active);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var n = NormalizeForSearch(query);
            filtered = filtered.Where(e =>
                NormalizeForSearch(e.Name).Contains(n) ||
                NormalizeForSearch(e.Code ?? string.Empty).Contains(n));
        }

        if (muscleGroupIds is { Count: > 0 })
            filtered = filtered.Where(e =>
                e.PrimaryMuscleIds.Any(id => muscleGroupIds.Contains(id)) ||
                e.SecondaryMuscleIds.Any(id => muscleGroupIds.Contains(id)));

        return filtered
            .Select(e => new ExerciseCatalogItem(e.Id, e.Name, e.PrimaryMuscleIds, null, string.Empty, string.Empty))
            .ToArray();
    }

    private static ExerciseDto Map(
        Exercise e,
        IReadOnlyDictionary<string, Muscle> muscleMap,
        IReadOnlyDictionary<string, MeasurementType> measurementTypeMap) => new(
        e.Id,
        e.Name,
        e.Code,
        e.Category,
        e.Difficulty,
        MapMuscles(e.PrimaryMuscleIds, muscleMap),
        MapMuscles(e.SecondaryMuscleIds, muscleMap),
        e.PrimaryMuscleIds,
        e.SecondaryMuscleIds,
        e.MeasurementTypeIds,
        e.MeasurementTypeIds
            .Select(id => measurementTypeMap.TryGetValue(id, out var measurementType) ? measurementType.Name : null)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray(),
        e.Active,
        e.IsDeleted,
        e.DeletedAt,
        e.CreatedAt,
        e.UpdatedAt);

    private async Task<IReadOnlyDictionary<string, Muscle>> BuildMuscleMapAsync(CancellationToken cancellationToken)
    {
        var muscles = await _muscleRepository.ListAsync(includeDeleted: true, cancellationToken);
        return muscles.ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyDictionary<string, MeasurementType>> BuildMeasurementTypeMapAsync(CancellationToken cancellationToken)
    {
        var measurementTypes = await _measurementTypeRepository.ListAsync(includeDeleted: true, cancellationToken);
        return measurementTypes.ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyCollection<ExerciseMuscleDto> MapMuscles(
        IEnumerable<string> muscleIds,
        IReadOnlyDictionary<string, Muscle> muscleMap)
    {
        return muscleIds
            .Select(id => muscleMap.TryGetValue(id, out var muscle) ? muscle : null)
            .Where(muscle => muscle is not null)
            .Select(muscle => new ExerciseMuscleDto(
                muscle!.Id,
                muscle.Name,
                muscle.Code,
                muscle.Description,
                muscle.MuscleGroupIds,
                muscle.Active,
                muscle.IsDeleted))
            .ToArray();
    }

    private static string NormalizeForSearch(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var nfd = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(nfd.Length);
        foreach (var ch in nfd)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        return sb.ToString().ToLowerInvariant();
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        var normalized = string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy.Trim().ToLowerInvariant();
        return AllowedSortBy.Contains(normalized) ? normalized : "name";
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        var normalized = string.IsNullOrWhiteSpace(sortDirection) ? "asc" : sortDirection.Trim().ToLowerInvariant();
        return AllowedSortDirection.Contains(normalized) ? normalized : "asc";
    }
}
