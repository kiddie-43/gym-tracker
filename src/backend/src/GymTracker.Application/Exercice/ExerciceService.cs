using GymTracker.Domain.Entities;

public sealed class ExerciceService
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "name",
        "description",
    };

    private static readonly HashSet<string> AllowedSortDirection = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc",
        "desc",
    };

    private readonly IExerciceRepository _repository;
    private readonly ExerciceCsvImportService _csvImportService;

    public ExerciceService(IExerciceRepository repository, ExerciceCsvImportService csvImportService)
    {
        _repository = repository;
        _csvImportService = csvImportService;
    }

    public async Task<ExercicePageResponse> ListPageAsync(
        string? code = null,
        string? name = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 0 ? 0 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        IEnumerable<Exercice> query = (await _repository.ListAsync(cancellationToken)).Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = Normalize(code);
            query = query.Where(exercice => Normalize(exercice.Code).Contains(normalizedCode, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalizedName = Normalize(name);
            query = query.Where(exercice => Normalize(exercice.Name).Contains(normalizedName, StringComparison.Ordinal));
        }

        IOrderedEnumerable<Exercice> ordered = normalizedSortBy switch
        {
            "code" => normalizedSortDirection == "desc"
                ? query.OrderByDescending(exercice => exercice.Code, StringComparer.OrdinalIgnoreCase).ThenBy(exercice => exercice.Id)
                : query.OrderBy(exercice => exercice.Code, StringComparer.OrdinalIgnoreCase).ThenBy(exercice => exercice.Id),
            "description" => normalizedSortDirection == "desc"
                ? query.OrderByDescending(exercice => exercice.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(exercice => exercice.Id)
                : query.OrderBy(exercice => exercice.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(exercice => exercice.Id),
            _ => normalizedSortDirection == "desc"
                ? query.OrderByDescending(exercice => exercice.Name, StringComparer.OrdinalIgnoreCase).ThenBy(exercice => exercice.Id)
                : query.OrderBy(exercice => exercice.Name, StringComparer.OrdinalIgnoreCase).ThenBy(exercice => exercice.Id),
        };

        var totalCount = query.Count();
        var items = ordered
            .Skip(normalizedPage * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(Map)
            .ToArray();

        return new ExercicePageResponse(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<ExerciceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var exercice = await _repository.GetByIdAsync(id, cancellationToken);
        return exercice is null ? null : Map(exercice);
    }

    public async Task<ExerciceResponse> CreateAsync(
        CreateExerciceRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCreateRequest(request);
        await ValidateRelationsAsync(normalized.Units, normalized.PrimaryMuscles, normalized.SecondaryMuscles, cancellationToken);

        if (await _repository.ExistsActiveCodeAsync(normalized.Code, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        var exercice = await _repository.CreateAsync(normalized, cancellationToken);
        return Map(exercice);
    }

    public async Task<ExerciceResponse?> UpdateAsync(
        Guid id,
        UpdateExerciceRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var normalized = NormalizeUpdateRequest(request);
        await ValidateRelationsAsync(normalized.Units, normalized.PrimaryMuscles, normalized.SecondaryMuscles, cancellationToken);
        var exercice = await _repository.UpdateAsync(id, normalized, cancellationToken);
        return exercice is null ? null : Map(exercice);
    }

    public Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ExerciceResponse?> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _repository.ReactivateAsync(id, cancellationToken);
        if (!reactivated)
        {
            return null;
        }

        var exercice = await _repository.GetByIdAsync(id, cancellationToken);
        return exercice is null ? null : Map(exercice);
    }

    public Task<ImportExercicesResult> ImportCsvAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        return _csvImportService.ImportAsync(stream, cancellationToken);
    }

    private static ExerciceResponse Map(Exercice exercice)
    {
        return new ExerciceResponse(
            exercice.Id,
            exercice.Name,
            exercice.Code,
            exercice.Description,
            exercice.Units
                .Where(x => x.DeletedAt == null)
                .Select(x => new ExerciceReferenceResponse(x.UnitId, x.Unit.Name))
                .ToArray(),
            exercice.Muscles
                .Where(x => x.DeletedAt == null && x.Type == ExerciceMuscleType.Primary)
                .Select(x => new ExerciceReferenceResponse(x.MuscleId, x.Muscle.Name))
                .ToArray(),
            exercice.Muscles
                .Where(x => x.DeletedAt == null && x.Type == ExerciceMuscleType.Secondary)
                .Select(x => new ExerciceReferenceResponse(x.MuscleId, x.Muscle.Name))
                .ToArray());
    }

    private static CreateExerciceRequest NormalizeCreateRequest(CreateExerciceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new CreateExerciceRequest(
            RequiredTrimmed(request.Name, nameof(request.Name)),
            RequiredUpperCode(request.Code, nameof(request.Code)),
            OptionalTrimmed(request.Description),
            request.Units ?? [],
            request.PrimaryMuscles ?? [],
            request.SecondaryMuscles ?? []);
    }

    private static UpdateExerciceRequest NormalizeUpdateRequest(UpdateExerciceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new UpdateExerciceRequest(
            RequiredTrimmed(request.Name, nameof(request.Name)),
            OptionalTrimmed(request.Description),
            request.Units ?? [],
            request.PrimaryMuscles ?? [],
            request.SecondaryMuscles ?? []);
    }

    private static string RequiredTrimmed(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        return value.Trim();
    }

    private static string RequiredUpperCode(string? value, string paramName)
    {
        var normalized = RequiredTrimmed(value, paramName);
        return normalized.ToUpperInvariant();
    }

    private static string? OptionalTrimmed(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
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

    private async Task ValidateRelationsAsync(
        IReadOnlyCollection<Guid> unitIds,
        IReadOnlyCollection<Guid> primaryMuscleIds,
        IReadOnlyCollection<Guid> secondaryMuscleIds,
        CancellationToken cancellationToken)
    {
        ValidateRequiredIds(unitIds, nameof(CreateExerciceRequest.Units), "Debe seleccionar al menos una unidad.");
        ValidateRequiredIds(primaryMuscleIds, nameof(CreateExerciceRequest.PrimaryMuscles), "Debe seleccionar al menos un musculo primario.");
        ValidateOptionalIds(secondaryMuscleIds, nameof(CreateExerciceRequest.SecondaryMuscles));

        if (!await _repository.UnitsExistAsync(unitIds, cancellationToken))
        {
            throw new ArgumentException("Una o mas unidades seleccionadas no existen.", nameof(CreateExerciceRequest.Units));
        }

        if (!await _repository.MusclesExistAsync(primaryMuscleIds, cancellationToken))
        {
            throw new ArgumentException("Uno o mas musculos primarios seleccionados no existen.", nameof(CreateExerciceRequest.PrimaryMuscles));
        }

        if (!await _repository.MusclesExistAsync(secondaryMuscleIds, cancellationToken))
        {
            throw new ArgumentException("Uno o mas musculos secundarios seleccionados no existen.", nameof(CreateExerciceRequest.SecondaryMuscles));
        }
    }

    private static void ValidateRequiredIds(
        IReadOnlyCollection<Guid> references,
        string paramName,
        string requiredMessage)
    {
        if (references is null || references.Count == 0)
        {
            throw new ArgumentException(requiredMessage, paramName);
        }

        if (references.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException($"{paramName} contains an invalid id.", paramName);
        }
    }

    private static void ValidateOptionalIds(
        IReadOnlyCollection<Guid> references,
        string paramName)
    {
        if (references is null)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        if (references.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException($"{paramName} contains an invalid id.", paramName);
        }
    }
}

