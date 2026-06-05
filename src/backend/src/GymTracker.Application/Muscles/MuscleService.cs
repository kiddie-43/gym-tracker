using GymTracker.Application.Admin.Common;
using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Muscles;

public sealed class MuscleService
{
    private static readonly string[] DefaultMuscleGroupIds = ["general"];
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

    private readonly IMuscleRepository _repository;
    private readonly MuscleCsvImportService _csvImportService;

    public MuscleService(IMuscleRepository repository, MuscleCsvImportService csvImportService)
    {
        _repository = repository;
        _csvImportService = csvImportService;
    }

    public async Task<MusclesPageResponse> ListPageAsync(
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

        IEnumerable<Muscle> query = (await _repository.ListAsync(cancellationToken)).Where(m => m.DeletedAt == null);


        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = Normalize(code);
            query = query.Where(muscle => Normalize(muscle.Code).Contains(normalizedCode, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalizedName = Normalize(name);
            query = query.Where(muscle => Normalize(muscle.Name).Contains(normalizedName, StringComparison.Ordinal));
        }

        IOrderedEnumerable<Muscle> ordered = normalizedSortBy switch
        {
            "code" => normalizedSortDirection == "desc"
                ? query.OrderByDescending(muscle => muscle.Code, StringComparer.OrdinalIgnoreCase).ThenBy(muscle => muscle.Id)
                : query.OrderBy(muscle => muscle.Code, StringComparer.OrdinalIgnoreCase).ThenBy(muscle => muscle.Id),
            "description" => normalizedSortDirection == "desc"
                ? query.OrderByDescending(muscle => muscle.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(muscle => muscle.Id)
                : query.OrderBy(muscle => muscle.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(muscle => muscle.Id),
            _ => normalizedSortDirection == "desc"
                ? query.OrderByDescending(muscle => muscle.Name, StringComparer.OrdinalIgnoreCase).ThenBy(muscle => muscle.Id)
                : query.OrderBy(muscle => muscle.Name, StringComparer.OrdinalIgnoreCase).ThenBy(muscle => muscle.Id),
        };

        var totalCount = query.Count();
        var items = ordered
            .Skip(normalizedPage * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(Map)
            .ToArray();

        return new MusclesPageResponse(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<MuscleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var muscle = await _repository.GetByIdAsync(id, cancellationToken);

        if (muscle is null)
            return null;

        return Map(muscle);
    }
    public async Task<MuscleResponse> CreateAsync(
        CreateMuscleRequest request,
        CancellationToken cancellationToken = default)
    {
            var normalized = NormalizeCreateRequest(request);

            if (await _repository.ExistsActiveCodeAsync(normalized.Code, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException("Code already exists among active records.");
            }

        var muscle = Muscle.Create(
                normalized.Name,
                normalized.Code,
                normalized.Description);



        await _repository.SaveAsync(muscle, cancellationToken);

        return Map(muscle);
    }
    public async Task<MuscleResponse?> UpdateAsync(
        Guid id,
        UpdateMuscleRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeUpdateRequest(request);
        var muscle = await _repository.GetByIdAsync(id, cancellationToken);

        if (muscle is null)
            return null;

        muscle.Update(
            normalized.Name,
            normalized.Description);

        await _repository.SaveAsync(muscle, cancellationToken);

        return Map(muscle);
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<MuscleResponse?> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _repository.ReactivateAsync(id, cancellationToken);
        if (!reactivated)
        {
            return null;
        }

        var muscle = await _repository.GetByIdAsync(id, cancellationToken);
        return muscle is null ? null : Map(muscle);
    }

  public Task<ImportMusclesResult> ImportCsvAsync(
    Stream stream,
    CancellationToken cancellationToken = default)
{

    return _csvImportService.ImportAsync(stream, cancellationToken);
}

    private static CreateMuscleRequest NormalizeCreateRequest(CreateMuscleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new CreateMuscleRequest(
            AdminRequestSanitizer.RequiredTrimmed(request.Name, nameof(request.Name)),
            AdminRequestSanitizer.RequiredUpperCode(request.Code, nameof(request.Code)),
            AdminRequestSanitizer.OptionalTrimmed(request.Description)
            );
    }

    private static UpdateMuscleRequest NormalizeUpdateRequest(UpdateMuscleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new UpdateMuscleRequest(
            AdminRequestSanitizer.RequiredTrimmed(request.Name, nameof(request.Name)),
            AdminRequestSanitizer.OptionalTrimmed(request.Description)
            );
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

    private static MuscleResponse Map(Muscle muscle)
    {
        return new MuscleResponse(
            muscle.Id,
            muscle.Name,
            muscle.Code,
            muscle.Description
        );
    }
}
