using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Admin.Exercises;

public sealed class ExerciseService
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "name",
        "category",
        "difficulty",
    };

    private static readonly HashSet<string> AllowedSortDirection = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc",
        "desc",
    };

    private readonly IExerciseRepository _repository;
    private readonly IExerciseStorageService _storageService;
    private readonly ExerciseMediaCompensationService _compensationService;
    private readonly IExerciseRelationsValidator _relationsValidator;

    public ExerciseService(
        IExerciseRepository repository,
        IExerciseStorageService storageService,
        ExerciseMediaCompensationService compensationService,
        IExerciseRelationsValidator relationsValidator)
    {
        _repository = repository;
        _storageService = storageService;
        _compensationService = compensationService;
        _relationsValidator = relationsValidator;
    }

    public async Task<IReadOnlyCollection<ExerciseResponse>> ListAsync(bool includeDeleted, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(includeDeleted, cancellationToken);
        return entities.Select(Map).ToArray();
    }

    public async Task<ExercisesPageResponse> ListPageAsync(
        bool includeDeleted,
        string? search = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var rows = await ListAsync(includeDeleted, cancellationToken);
        var filteredRows = rows
            .Where(row => string.IsNullOrWhiteSpace(search)
                || row.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                || row.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || row.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
                || row.Difficulty.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var comparer = StringComparer.OrdinalIgnoreCase;

        IOrderedEnumerable<ExerciseResponse> orderedRows = normalizedSortBy switch
        {
            "code" => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filteredRows.OrderByDescending(row => row.Code, comparer).ThenBy(row => row.Id, comparer)
                : filteredRows.OrderBy(row => row.Code, comparer).ThenBy(row => row.Id, comparer),
            "category" => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filteredRows.OrderByDescending(row => row.Category, comparer).ThenBy(row => row.Id, comparer)
                : filteredRows.OrderBy(row => row.Category, comparer).ThenBy(row => row.Id, comparer),
            "difficulty" => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filteredRows.OrderByDescending(row => row.Difficulty, comparer).ThenBy(row => row.Id, comparer)
                : filteredRows.OrderBy(row => row.Difficulty, comparer).ThenBy(row => row.Id, comparer),
            _ => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filteredRows.OrderByDescending(row => row.Name, comparer).ThenBy(row => row.Id, comparer)
                : filteredRows.OrderBy(row => row.Name, comparer).ThenBy(row => row.Id, comparer),
        };

        var totalCount = filteredRows.Length;
        var pageRows = orderedRows
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToArray();

        return new ExercisesPageResponse(pageRows, totalCount, normalizedPage, normalizedPageSize);
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

    public async Task<ExerciseResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null || entity.IsDeleted ? null : Map(entity);
    }

    public async Task<ExerciseResponse> CreateAsync(UpsertExerciseRequest request, CancellationToken cancellationToken = default)
    {
        await _relationsValidator.ValidateAsync(request, cancellationToken);

        var hasDuplicateCode = await _repository.ExistsActiveCodeAsync(request.Code, cancellationToken: cancellationToken);
        if (hasDuplicateCode)
        {
            throw new InvalidOperationException("Duplicate code is not allowed.");
        }

        var entity = Exercise.Create(
            request.Name,
            request.Code,
            request.Description,
            request.Category,
            request.Difficulty,
            request.MeasurementTypeIds,
            request.MeasurementTypeCode,
            request.PrimaryMuscleIds,
            request.SecondaryMuscleIds,
            request.MuscleGroupIds,
            media: Array.Empty<ExerciseMedia>());

        entity.Update(
            request.Name,
            request.Code,
            request.Description,
            request.Category,
            request.Difficulty,
            request.MeasurementTypeIds,
            request.MeasurementTypeCode,
            request.PrimaryMuscleIds,
            request.SecondaryMuscleIds,
            request.MuscleGroupIds,
            request.Active);

        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<ExerciseResponse?> UpdateAsync(string id, UpsertExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted)
        {
            return null;
        }

        await _relationsValidator.ValidateAsync(request, cancellationToken);

        var hasDuplicateCode = await _repository.ExistsActiveCodeAsync(request.Code, id, cancellationToken);
        if (hasDuplicateCode)
        {
            throw new InvalidOperationException("Duplicate code is not allowed.");
        }

        entity.Update(
            request.Name,
            request.Code,
            request.Description,
            request.Category,
            request.Difficulty,
            request.MeasurementTypeIds,
            request.MeasurementTypeCode,
            request.PrimaryMuscleIds,
            request.SecondaryMuscleIds,
            request.MuscleGroupIds,
            request.Active);

        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task<ExerciseResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var hasDuplicateCode = await _repository.ExistsActiveCodeAsync(entity.Code, id, cancellationToken);
        if (hasDuplicateCode)
        {
            throw new InvalidOperationException("Duplicate code is not allowed.");
        }

        var reactivateRequest = new UpsertExerciseRequest(
            entity.Name,
            entity.Code,
            entity.Description,
            entity.Category,
            entity.Difficulty,
            entity.MeasurementTypeId,
            entity.MeasurementTypeCode,
            entity.PrimaryMuscleIds,
            entity.SecondaryMuscleIds,
            entity.MuscleGroupIds,
            Active: true);

        await _relationsValidator.ValidateAsync(reactivateRequest, cancellationToken);

        await _repository.ReactivateAsync(id, cancellationToken);
        return Map(entity);
    }

    public async Task<MediaUploadTicket> RequestUploadUrlAsync(string exerciseId, RequestUploadUrlRequest request, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(exerciseId, cancellationToken);
        if (exercise is null || exercise.IsDeleted)
        {
            throw new ArgumentException("Exercise not found.", nameof(exerciseId));
        }

        var mediaId = Guid.NewGuid().ToString("N");
        var safeFileName = request.FileName.Replace(' ', '-');
        var storagePath = $"admin/exercises/{exerciseId}/{mediaId}/{safeFileName}";
        var expiresIn = TimeSpan.FromMinutes(10);
        var uploadUrl = await _storageService.GenerateUploadUrlAsync(storagePath, expiresIn, request.ContentType, cancellationToken);

        return new MediaUploadTicket(
            ExerciseId: exerciseId,
            MediaId: mediaId,
            StoragePath: storagePath,
            UploadUrl: uploadUrl,
            ExpiresAt: DateTimeOffset.UtcNow.Add(expiresIn),
            ContentType: request.ContentType,
            MaxSizeBytes: request.MediaType == ExerciseMediaType.Video ? 100 * 1024 * 1024 : 5 * 1024 * 1024);
    }

    public async Task<ExerciseResponse> ConfirmMediaAsync(string exerciseId, ConfirmExerciseMediaRequest request, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(exerciseId, cancellationToken);
        if (exercise is null || exercise.IsDeleted)
        {
            throw new ArgumentException("Exercise not found.", nameof(exerciseId));
        }

        var exists = await _storageService.ExistsAsync(request.StoragePath, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException("Storage object does not exist.", nameof(request.StoragePath));
        }

        var mediaItems = exercise.Media.ToList();
        var item = ExerciseMedia.Create(
            request.MediaId,
            request.MediaType,
            request.Title ?? request.FileName,
            request.StoragePath,
            request.ThumbnailPath,
            request.ContentType,
            request.FileName,
            request.SizeBytes,
            request.SortOrder,
            request.IsPrimary,
            DateTimeOffset.UtcNow);

        mediaItems.RemoveAll(existing => string.Equals(existing.MediaId, request.MediaId, StringComparison.OrdinalIgnoreCase));
        mediaItems.Add(item);

        try
        {
            exercise.ReplaceMedia(mediaItems.ToArray());
            await _repository.SaveAsync(exercise, cancellationToken);
        }
        catch
        {
            await _compensationService.CompensateDeleteAsync(request.StoragePath, cancellationToken);
            throw;
        }

        return Map(exercise);
    }

    public async Task<ExerciseResponse> ReorderMediaAsync(string exerciseId, ReorderExerciseMediaRequest request, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(exerciseId, cancellationToken)
            ?? throw new ArgumentException("Exercise not found.", nameof(exerciseId));

        var map = request.Items.ToDictionary(item => item.MediaId, item => item.SortOrder, StringComparer.OrdinalIgnoreCase);
        var next = exercise.Media
            .Select(item => map.TryGetValue(item.MediaId, out var sortOrder)
                ? item with { SortOrder = sortOrder, UpdatedAt = DateTimeOffset.UtcNow }
                : item)
            .ToArray();

        exercise.ReplaceMedia(next);
        await _repository.SaveAsync(exercise, cancellationToken);
        return Map(exercise);
    }

    public async Task<ExerciseResponse> SetPrimaryMediaAsync(string exerciseId, SetPrimaryExerciseMediaRequest request, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(exerciseId, cancellationToken)
            ?? throw new ArgumentException("Exercise not found.", nameof(exerciseId));

        var next = exercise.Media
            .Select(item => item with
            {
                IsPrimary = string.Equals(item.MediaId, request.MediaId, StringComparison.OrdinalIgnoreCase),
                UpdatedAt = DateTimeOffset.UtcNow,
            })
            .ToArray();

        exercise.ReplaceMedia(next);
        await _repository.SaveAsync(exercise, cancellationToken);
        return Map(exercise);
    }

    public async Task<ExerciseResponse> UpdateMediaAsync(string exerciseId, string mediaId, UpdateExerciseMediaRequest request, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(exerciseId, cancellationToken)
            ?? throw new ArgumentException("Exercise not found.", nameof(exerciseId));

        var next = exercise.Media
            .Select(item => !string.Equals(item.MediaId, mediaId, StringComparison.OrdinalIgnoreCase)
                ? item
                : item with
                {
                    Title = string.IsNullOrWhiteSpace(request.Title) ? item.Title : request.Title,
                    Active = request.Active ?? item.Active,
                    UpdatedAt = DateTimeOffset.UtcNow,
                })
            .ToArray();

        exercise.ReplaceMedia(next);
        await _repository.SaveAsync(exercise, cancellationToken);
        return Map(exercise);
    }

    public async Task<ExerciseResponse> DeleteMediaAsync(string exerciseId, string mediaId, CancellationToken cancellationToken = default)
    {
        var exercise = await _repository.GetByIdAsync(exerciseId, cancellationToken)
            ?? throw new ArgumentException("Exercise not found.", nameof(exerciseId));

        var toDelete = exercise.Media.FirstOrDefault(item => string.Equals(item.MediaId, mediaId, StringComparison.OrdinalIgnoreCase));
        if (toDelete is null)
        {
            throw new ArgumentException("Media not found.", nameof(mediaId));
        }

        await _storageService.DeleteAsync(toDelete.StoragePath, cancellationToken);

        var next = exercise.Media
            .Where(item => !string.Equals(item.MediaId, mediaId, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        exercise.ReplaceMedia(next);
        await _repository.SaveAsync(exercise, cancellationToken);
        return Map(exercise);
    }

    public async Task<IReadOnlyCollection<WorkoutCatalogExerciseDto>> ListWorkoutCatalogAsync(string? query, IReadOnlyCollection<string>? muscleGroupIds, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.ListAsync(includeDeleted: false, cancellationToken: cancellationToken);

        var normalizedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        var groupSet = muscleGroupIds is { Count: > 0 }
            ? muscleGroupIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : null;

        return rows
            .Where(row => row.Active && !row.IsDeleted)
            .Where(row => normalizedQuery is null || row.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) || row.Code.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .Where(row => groupSet is null || row.MuscleGroupIds.Any(id => groupSet.Contains(id)))
            .Select(row => new WorkoutCatalogExerciseDto(
                row.Id,
                row.Name,
                row.MuscleGroupIds,
                row.ResolveCoverStoragePath(),
                row.MeasurementTypeId,
                row.MeasurementTypeCode))
            .ToArray();
    }

    public async Task<ImportExercisesResult> ImportCsvAsync(ImportExercisesRequest request, CancellationToken cancellationToken = default)
    {
        var rows = request.Rows ?? Array.Empty<ImportExerciseRowRequest>();
        var results = new List<ImportExerciseRowResult>(rows.Count);
        var createdRows = 0;

        var existingActiveCodes = (await _repository.ListAsync(includeDeleted: false, cancellationToken))
            .Select(item => item.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var batchCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rowNumber = 1;
        foreach (var row in rows)
        {
            var normalizedCode = NormalizeCode(row.Code);
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                results.Add(new ImportExerciseRowResult(rowNumber, row.Code, false, "Code is required.", null));
                rowNumber++;
                continue;
            }

            if (!batchCodes.Add(normalizedCode))
            {
                results.Add(new ImportExerciseRowResult(rowNumber, normalizedCode, false, "Duplicate code in CSV payload.", null));
                rowNumber++;
                continue;
            }

            if (existingActiveCodes.Contains(normalizedCode))
            {
                results.Add(new ImportExerciseRowResult(rowNumber, normalizedCode, false, "Duplicate code against existing active records.", null));
                rowNumber++;
                continue;
            }

            var primaryMuscleIds = NormalizeIds(row.PrimaryMuscleIds);
            if (primaryMuscleIds.Length == 0)
            {
                results.Add(new ImportExerciseRowResult(rowNumber, normalizedCode, false, "At least one primary muscle is required.", null));
                rowNumber++;
                continue;
            }

            var requestRow = new UpsertExerciseRequest(
                Name: string.IsNullOrWhiteSpace(row.Name) ? normalizedCode : row.Name.Trim(),
                Code: normalizedCode,
                Description: string.IsNullOrWhiteSpace(row.Description) ? null : row.Description.Trim(),
                Category: string.IsNullOrWhiteSpace(row.Category) ? "Strength" : row.Category.Trim(),
                Difficulty: string.IsNullOrWhiteSpace(row.Difficulty) ? "Beginner" : row.Difficulty.Trim(),
                MeasurementTypeId: row.MeasurementTypeId?.Trim() ?? string.Empty,
                MeasurementTypeCode: row.MeasurementTypeCode?.Trim() ?? string.Empty,
                PrimaryMuscleIds: primaryMuscleIds,
                SecondaryMuscleIds: NormalizeIds(row.SecondaryMuscleIds),
                MuscleGroupIds: primaryMuscleIds,
                Active: true);

            try
            {
                var created = await CreateAsync(requestRow, cancellationToken);
                existingActiveCodes.Add(normalizedCode);
                createdRows++;
                results.Add(new ImportExerciseRowResult(rowNumber, normalizedCode, true, null, created));
            }
            catch (ArgumentException exception)
            {
                results.Add(new ImportExerciseRowResult(rowNumber, normalizedCode, false, exception.Message, null));
            }
            catch (InvalidOperationException exception)
            {
                results.Add(new ImportExerciseRowResult(rowNumber, normalizedCode, false, exception.Message, null));
            }

            rowNumber++;
        }

        return new ImportExercisesResult(
            TotalRows: rows.Count,
            CreatedRows: createdRows,
            RejectedRows: rows.Count - createdRows,
            Rows: results);
    }

    private static string NormalizeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private static string[] NormalizeIds(IReadOnlyCollection<string>? values)
    {
        if (values is null)
        {
            return Array.Empty<string>();
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static ExerciseResponse Map(Exercise entity)
    {
        return new ExerciseResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.Category,
            entity.Difficulty,
            entity.MeasurementTypeId,
            entity.MeasurementTypeCode,
            entity.PrimaryMuscleIds,
            entity.SecondaryMuscleIds,
            entity.MuscleGroupIds,
            entity.ResolveCoverStoragePath(),
            entity.Active,
            entity.IsDeleted,
            entity.Media,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt);
    }
}
