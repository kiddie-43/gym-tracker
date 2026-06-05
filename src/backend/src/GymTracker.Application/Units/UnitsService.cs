using GymTracker.Application.Admin.Common;
using DomainUnit = GymTracker.Domain.Entities.Units;

namespace GymTracker.Application.Units;
public sealed class UnitsService
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

    private readonly IUnitsRepository _repository;
    private readonly UnitsCsvImportService _csvImportService;

    public UnitsService(IUnitsRepository repository, UnitsCsvImportService csvImportService)
    {
        _repository = repository;
        _csvImportService = csvImportService;
    }

    public async Task<UnitsPageResponse> ListPageAsync(
        string? code = null,
        string? name = null,
        string? description = null,
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

        IEnumerable<DomainUnit> query = await _repository.ListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = Normalize(code);
            query = query.Where(unit => Normalize(unit.Code).Contains(normalizedCode, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalizedName = Normalize(name);
            query = query.Where(unit => Normalize(unit.Name).Contains(normalizedName, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            var normalizedDescription = Normalize(description);
            query = query.Where(unit => Normalize(unit.Description).Contains(normalizedDescription, StringComparison.Ordinal));
        }

        IOrderedEnumerable<DomainUnit> ordered = normalizedSortBy switch
        {
            "code" => normalizedSortDirection == "desc"
                ? query.OrderByDescending(unit => unit.Code, StringComparer.OrdinalIgnoreCase).ThenBy(unit => unit.Id)
                : query.OrderBy(unit => unit.Code, StringComparer.OrdinalIgnoreCase).ThenBy(unit => unit.Id),
            "description" => normalizedSortDirection == "desc"
                ? query.OrderByDescending(unit => unit.Description, StringComparer.OrdinalIgnoreCase).ThenBy(unit => unit.Id)
                : query.OrderBy(unit => unit.Description, StringComparer.OrdinalIgnoreCase).ThenBy(unit => unit.Id),
            _ => normalizedSortDirection == "desc"
                ? query.OrderByDescending(unit => unit.Name, StringComparer.OrdinalIgnoreCase).ThenBy(unit => unit.Id)
                : query.OrderBy(unit => unit.Name, StringComparer.OrdinalIgnoreCase).ThenBy(unit => unit.Id),
        };

        var totalCount = query.Count();
        var items = ordered
            .Skip(normalizedPage * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(Map)
            .ToArray();

        return new UnitsPageResponse(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<UnitsResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unit = await _repository.GetByIdAsync(id, cancellationToken);

        if (unit is null)
            return null;

        return Map(unit);
    }
    public async Task<UnitsResponse> CreateAsync(
        CreateUnitsRequest request,
        CancellationToken cancellationToken = default)
    {
            var normalized = NormalizeCreateRequest(request);

            if (await _repository.ExistsByCodeAsync(normalized.Code, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException("Code already exists among active records.");
            }

        var unit = DomainUnit.Create(
                normalized.Code,
                normalized.Name,
                normalized.Description);

            

        await _repository.SaveAsync(unit, cancellationToken);

        return Map(unit);
    }
    public async Task<UnitsResponse?> UpdateAsync(
        Guid id,
        UpsertUnitsRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeUpdateRequest(request);
        var unit = await _repository.GetByIdAsync(id, cancellationToken);

        if (unit is null)
            return null;

        

        unit.Update(
            normalized.Name,
            normalized.Description);

        

        await _repository.SaveAsync(unit, cancellationToken);

        return Map(unit);
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<UnitsResponse?> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _repository.ReactivateAsync(id, cancellationToken);
        if (!reactivated)
        {
            return null;
        }

        var unit = await _repository.GetByIdAsync(id, cancellationToken);
        return unit is null ? null : Map(unit);
    }
 public Task<ImportUnitsResult> ImportCsvAsync(
    Stream stream,
    CancellationToken cancellationToken = default)
{

    return _csvImportService.ImportAsync(stream, cancellationToken);
}
    private static UpsertUnitsRequest NormalizeUpdateRequest(UpsertUnitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new UpsertUnitsRequest(
            request.Name.Trim(),
            request.Description.Trim()
            );
    }
   private static CreateUnitsRequest NormalizeCreateRequest(CreateUnitsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new CreateUnitsRequest(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Description.Trim()
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

    private static UnitsResponse Map(DomainUnit unit)
    {
        return new UnitsResponse(
            unit.Id,
            unit.Code,
            unit.Name,
            unit.Description
            );
    }
}
