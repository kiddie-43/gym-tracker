using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.MeasurementTypes;

public sealed class MeasurementTypeService
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "code", "name", "description",
    };

    private static readonly HashSet<string> AllowedSortDirection = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc", "desc",
    };

    private readonly IMeasurementTypeRepository _repository;

    public MeasurementTypeService(IMeasurementTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<MeasurementTypesPageResponse> ListPageAsync(
        bool includeDeleted = false,
        string? search = null,
        string? code = null,
        string sortBy = "code",
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
            filtered = filtered.Where(e =>
                e.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (e.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            filtered = filtered.Where(e =>
                e.Code.Contains(code, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var comparer = StringComparer.OrdinalIgnoreCase;
        IOrderedEnumerable<MeasurementType> ordered = normalizedSortBy switch
        {
            "name" => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Name, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Name, comparer).ThenBy(e => e.Id, comparer),
            "description" => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Description ?? string.Empty, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Description ?? string.Empty, comparer).ThenBy(e => e.Id, comparer),
            _ => normalizedSortDirection == "desc"
                ? filtered.OrderByDescending(e => e.Code, comparer).ThenBy(e => e.Id, comparer)
                : filtered.OrderBy(e => e.Code, comparer).ThenBy(e => e.Id, comparer),
        };

        var total = filtered.Count;
        var items = ordered
            .Skip(normalizedPage * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(Map)
            .ToArray();

        return new MeasurementTypesPageResponse(items, total, normalizedPage, normalizedPageSize);
    }

    public async Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> SearchAsync(
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(includeDeleted: false, cancellationToken);
        IEnumerable<MeasurementType> filtered = entities.Where(e => !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
        {
            filtered = filtered.Where(e =>
                e.Code.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                e.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return filtered
            .OrderBy(e => e.Code, StringComparer.OrdinalIgnoreCase)
            .Select(e => new AssignableMeasurementTypeResponse(e.Id, e.Code, e.Name, e.Description))
            .ToArray();
    }

    public async Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null || entity.IsDeleted ? null : Map(entity);
    }

    public async Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        var (code, name) = NormalizeRequest(request);

        if (await _repository.ExistsActiveCodeAsync(code, cancellationToken: cancellationToken))
            throw new InvalidOperationException("Code already exists among active records.");

        var entity = MeasurementType.Create(code, name, request.Description);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted) return null;

        var (code, name) = NormalizeRequest(request);

        if (await _repository.ExistsActiveCodeAsync(code, id, cancellationToken))
            throw new InvalidOperationException("Code already exists among active records.");

        entity.Update(code, name, request.Description);
        await _repository.SaveAsync(entity, cancellationToken);
        return Map(entity);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, DateTimeOffset.UtcNow, cancellationToken);

    public async Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var reactivated = await _repository.ReactivateAsync(id, cancellationToken);
        if (!reactivated) return null;
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<ImportMeasurementTypesResult> ImportCsvAsync(
        ImportMeasurementTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        var rows = request.Rows ?? Array.Empty<ImportMeasurementTypeRowRequest>();
        var results = new List<ImportMeasurementTypeRowResult>(rows.Count);
        var createdRows = 0;
        var rowNumber = 1;

        foreach (var row in rows)
        {
            try
            {
                var created = await CreateAsync(
                    new UpsertMeasurementTypeRequest { Code = row.Code, Name = row.Name, Description = row.Description },
                    cancellationToken);
                createdRows++;
                results.Add(new ImportMeasurementTypeRowResult(rowNumber, created.Code, true, null, created));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                results.Add(new ImportMeasurementTypeRowResult(rowNumber,
                    string.IsNullOrWhiteSpace(row.Code) ? null : row.Code.Trim(), false, ex.Message, null));
            }

            rowNumber++;
        }

        return new ImportMeasurementTypesResult(rows.Count, createdRows, rows.Count - createdRows, results);
    }

    private static MeasurementTypeResponse Map(MeasurementType e) =>
        new(e.Id, e.Code, e.Name, e.Description, e.IsDeleted);

    private static (string Code, string Name) NormalizeRequest(UpsertMeasurementTypeRequest request)
    {
        var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(request.Code));
        var name = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = code;
        return (code, name);
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        var normalized = string.IsNullOrWhiteSpace(sortBy) ? "code" : sortBy.Trim().ToLowerInvariant();
        return AllowedSortBy.Contains(normalized) ? normalized : "code";
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        var normalized = string.IsNullOrWhiteSpace(sortDirection) ? "asc" : sortDirection.Trim().ToLowerInvariant();
        return AllowedSortDirection.Contains(normalized) ? normalized : "asc";
    }
}

