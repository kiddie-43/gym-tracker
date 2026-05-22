using System.Collections.Concurrent;

using GymTracker.Application.Admin.MeasurementTypes;

namespace GymTracker.Infrastructure.Firebase;

public sealed class InMemoryMeasurementTypeRepository : IMeasurementTypeRepository
{
    private static readonly ConcurrentDictionary<string, MeasurementTypeResponse> Store = new(StringComparer.OrdinalIgnoreCase);

    public Task<MeasurementTypesPageResponse> ListPageAsync(
        bool includeInactive = false,
        string? search = null,
        string? code = null,
        string sortBy = "code",
        string sortDirection = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var rows = includeInactive
            ? Store.Values.ToArray()
            : Store.Values.Where(item => !item.IsDeleted).ToArray();

        if (!string.IsNullOrWhiteSpace(search))
        {
            rows = rows.Where(item =>
                    item.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (item.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToArray();
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            rows = rows.Where(item => item.Code.Contains(code, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        var ordered = sortBy.ToLowerInvariant() switch
        {
            "name" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Name, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
            "description" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
            _ => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Code, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
        };

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;
        var totalCount = rows.Length;
        var pagedItems = ordered.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize).ToArray();

        return Task.FromResult(new MeasurementTypesPageResponse(pagedItems, totalCount, normalizedPage, normalizedPageSize));
    }

    public Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
    {
        var rows = Store.Values
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .Select(item => new AssignableMeasurementTypeResponse(item.Id, item.Code, item.Name, item.Description))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<AssignableMeasurementTypeResponse>>(rows);
    }

    public Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Store.TryGetValue(id, out var row) || row.IsDeleted)
        {
            return Task.FromResult<MeasurementTypeResponse?>(null);
        }

        return Task.FromResult<MeasurementTypeResponse?>(row);
    }

    public Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        var (code, name) = NormalizeRequest(request);
        var hasConflict = Store.Values.Any(item =>
            !item.IsDeleted &&
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));

        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        var row = new MeasurementTypeResponse(
            Id: Guid.NewGuid().ToString("N"),
            Code: code,
            Name: name,
            Description: string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsDeleted: false);

        Store[row.Id] = row;
        return Task.FromResult(row);
    }

    public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (!Store.TryGetValue(id, out var existing) || existing.IsDeleted)
        {
            return Task.FromResult<MeasurementTypeResponse?>(null);
        }

        var (code, name) = NormalizeRequest(request);
        var hasConflict = Store.Values.Any(item =>
            !item.IsDeleted &&
            !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));

        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        var updated = existing with
        {
            Code = code,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        };

        Store[id] = updated;
        return Task.FromResult<MeasurementTypeResponse?>(updated);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Store.TryGetValue(id, out var existing))
        {
            return Task.FromResult(false);
        }

        Store[id] = existing with { IsDeleted = true };
        return Task.FromResult(true);
    }

    public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!Store.TryGetValue(id, out var existing))
        {
            return Task.FromResult<MeasurementTypeResponse?>(null);
        }

        var hasConflict = Store.Values.Any(item =>
            !item.IsDeleted &&
            !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.Code, existing.Code, StringComparison.OrdinalIgnoreCase));

        if (hasConflict)
        {
            throw new InvalidOperationException("Code already exists among active records.");
        }

        var updated = existing with { IsDeleted = false };
        Store[id] = updated;
        return Task.FromResult<MeasurementTypeResponse?>(updated);
    }

    private static (string Code, string Name) NormalizeRequest(UpsertMeasurementTypeRequest request)
    {
        var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required.", nameof(request.Code));
        }

        var name = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = code;
        }

        return (code, name);
    }
}