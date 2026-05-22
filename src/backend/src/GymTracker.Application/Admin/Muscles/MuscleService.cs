using GymTracker.Application.Admin.Common;
using GymTracker.Domain.Entities;
using System.Globalization;
using System.Text;

namespace GymTracker.Application.Admin.Muscles;

public sealed class MuscleService
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

    private readonly IMuscleRepository _repository;
    private readonly MuscleCsvImportService _csvImportService;

    public MuscleService(IMuscleRepository repository, MuscleCsvImportService csvImportService)
    {
        _repository = repository;
        _csvImportService = csvImportService;
    }

    private static string NormalizeForSearch(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Normalize to NFD form and remove combining characters (accents)
        var nfdText = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in nfdText)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return sb.ToString().ToLowerInvariant();
    }

    public async Task<IReadOnlyCollection<MuscleResponse>> ListAsync(
        bool includeDeleted,
        string? search = null,
        string? filterCode = null,
        string? filterName = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(includeDeleted: true, cancellationToken);
        var filtered = AdminSoftDeleteFilter.Apply(entities, item => item.IsDeleted, item => item.Active, includeDeleted)
            .ToList(); // Materialize immediately

        // Apply text filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchNormalized = NormalizeForSearch(search);
            filtered = filtered
                .Where(m =>
                    NormalizeForSearch(m.Code).Contains(searchNormalized) ||
                    NormalizeForSearch(m.Name).Contains(searchNormalized) ||
                    NormalizeForSearch(m.Description ?? string.Empty).Contains(searchNormalized))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(filterCode))
        {
            var codeNormalized = NormalizeForSearch(filterCode);
            filtered = filtered
                .Where(m => NormalizeForSearch(m.Code).Contains(codeNormalized))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(filterName))
        {
            var nameNormalized = NormalizeForSearch(filterName);
            filtered = filtered
                .Where(m => NormalizeForSearch(m.Name).Contains(nameNormalized))
                .ToList();
        }

        return filtered.Select(Map).ToArray();
    }

    public async Task<MusclesPageResponse> ListPageAsync(
        bool includeDeleted,
        string? search = null,
        string? filterCode = null,
        string? filterName = null,
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

        var rows = await ListAsync(includeDeleted, search, filterCode, filterName, cancellationToken);
        var comparer = StringComparer.OrdinalIgnoreCase;
        IOrderedEnumerable<MuscleResponse> ordered = normalizedSortBy switch
        {
            "code" => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Code, comparer).ThenBy(item => item.Id, comparer)
                : rows.OrderBy(item => item.Code, comparer).ThenBy(item => item.Id, comparer),
            "description" => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Description ?? string.Empty, comparer).ThenBy(item => item.Id, comparer)
                : rows.OrderBy(item => item.Description ?? string.Empty, comparer).ThenBy(item => item.Id, comparer),
            _ => normalizedSortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Name, comparer).ThenBy(item => item.Id, comparer)
                : rows.OrderBy(item => item.Name, comparer).ThenBy(item => item.Id, comparer),
        };

        var totalCount = rows.Count;
        var pagedItems = ordered
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToArray();

        return new MusclesPageResponse(pagedItems, totalCount, normalizedPage, normalizedPageSize);
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        var normalized = string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy.Trim().ToLowerInvariant();
        if (!AllowedSortBy.Contains(normalized))
        {
            return "name";
        }

        return normalized;
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        var normalized = string.IsNullOrWhiteSpace(sortDirection) ? "asc" : sortDirection.Trim().ToLowerInvariant();
        if (!AllowedSortDirection.Contains(normalized))
        {
            return "asc";
        }

        return normalized;
    }

    public async Task<MuscleResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null || entity.IsDeleted ? null : Map(entity);
    }

    public async Task<MuscleResponse> CreateAsync(UpsertMuscleRequest request, CancellationToken cancellationToken = default)
    {
        var hasConflict = await _repository.ExistsActiveCodeAsync(request.Code, cancellationToken: cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        var entity = Muscle.Create(request.Name, request.Code, request.Description, request.MuscleGroupIds);
        if (!request.Active)
        {
            entity.Update(entity.Name, entity.Code, entity.Description, entity.MuscleGroupIds, active: false);
        }

        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<MuscleResponse?> UpdateAsync(string id, UpsertMuscleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted)
        {
            return null;
        }

        var hasConflict = await _repository.ExistsActiveCodeAsync(request.Code, id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        entity.Update(request.Name, request.Code, request.Description, request.MuscleGroupIds, request.Active);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);
    }

    public async Task<MuscleResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var activeCodes = (await _repository.ListAsync(includeDeleted: false, cancellationToken))
            .Where(item => !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Code);

        AdminReactivationGuard.EnsureNoActiveConflict(entity.Code, activeCodes, nameof(entity.Code));

        await _repository.ReactivateAsync(id, cancellationToken);
        return Map(entity);
    }

    public async Task<ImportMusclesResult> ImportCsvAsync(ImportMusclesRequest request, CancellationToken cancellationToken = default)
    {
        return await _csvImportService.ImportAsync(request, _repository, cancellationToken);
    }

    private static MuscleResponse Map(Muscle entity)
    {
        return new MuscleResponse(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.MuscleGroupIds,
            entity.Active,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
