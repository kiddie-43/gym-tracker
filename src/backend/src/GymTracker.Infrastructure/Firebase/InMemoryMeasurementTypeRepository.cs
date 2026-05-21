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
        string sortBy = "name",
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
                    item.Key.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || item.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (item.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToArray();
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            rows = rows.Where(item => item.Key.Contains(code, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        var ordered = sortBy.ToLowerInvariant() switch
        {
            "category" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Category, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Category, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
            "description" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Description ?? string.Empty, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
            "code" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Key, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
            "key" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Key, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
            _ => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? rows.OrderByDescending(item => item.Name, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
                : rows.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase),
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
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(item => new AssignableMeasurementTypeResponse(item.Id, item.Key, item.Name, item.Unit, item.DataType, item.Category))
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
        var normalized = NormalizeRequest(request);
        var hasConflict = Store.Values.Any(item =>
            !item.IsDeleted &&
            string.Equals(item.Key, normalized.Key, StringComparison.OrdinalIgnoreCase));

        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var now = DateTimeOffset.UtcNow;
        var row = new MeasurementTypeResponse(
            Id: Guid.NewGuid().ToString("N"),
            Key: normalized.Key,
            Name: normalized.Name,
            Unit: normalized.Unit,
            DataType: normalized.DataType,
            Category: normalized.Category,
            Description: normalized.Description,
            Active: true,
            IsDeleted: false,
            CreatedAt: now,
            UpdatedAt: now,
            DeletedAt: null);

        Store[row.Id] = row;
        return Task.FromResult(row);
    }

    public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (!Store.TryGetValue(id, out var existing) || existing.IsDeleted)
        {
            return Task.FromResult<MeasurementTypeResponse?>(null);
        }

        var normalized = NormalizeRequest(request);
        var hasConflict = Store.Values.Any(item =>
            !item.IsDeleted &&
            !string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.Key, normalized.Key, StringComparison.OrdinalIgnoreCase));

        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = existing with
        {
            Key = normalized.Key,
            Name = normalized.Name,
            Unit = normalized.Unit,
            DataType = normalized.DataType,
            Category = normalized.Category,
            Description = normalized.Description,
            UpdatedAt = DateTimeOffset.UtcNow,
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

        Store[id] = existing with
        {
            Active = false,
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

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
            string.Equals(item.Key, existing.Key, StringComparison.OrdinalIgnoreCase));

        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = existing with
        {
            Active = true,
            IsDeleted = false,
            DeletedAt = null,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        Store[id] = updated;
        return Task.FromResult<MeasurementTypeResponse?>(updated);
    }

    private static (string Key, string Name, string Unit, string DataType, string Category, string? Description) NormalizeRequest(UpsertMeasurementTypeRequest request)
    {
        var name = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(request.Name));
        }

        var inferred = InferDefaults(name);
        var key = string.IsNullOrWhiteSpace(request.Key)
            ? inferred.Key
            : request.Key.Trim().ToUpperInvariant();

        var hasLegacyFields = request.Fields is { Count: > 0 };

        var unit = string.IsNullOrWhiteSpace(request.Unit)
            ? (hasLegacyFields ? "count" : inferred.Unit)
            : request.Unit.Trim();

        var dataType = string.IsNullOrWhiteSpace(request.DataType)
            ? (hasLegacyFields ? "integer" : inferred.DataType)
            : request.DataType.Trim().ToLowerInvariant();

        var category = string.IsNullOrWhiteSpace(request.Category)
            ? (hasLegacyFields ? "general" : inferred.Category)
            : request.Category.Trim().ToLowerInvariant();

        return (key, name, unit, dataType, category, string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim());
    }

    private static (string Key, string Unit, string DataType, string Category) InferDefaults(string normalizedName)
    {
        var lower = normalizedName.ToLowerInvariant();

        if (lower.Contains("kilo") || lower.Contains("peso") || lower.Contains("kg"))
        {
            return ("PESO", "kg", "decimal", "strength");
        }

        if (lower.Contains("rep"))
        {
            return ("REPETICIONES", "reps", "integer", "strength");
        }

        if (lower.Contains("dist") || lower.Contains("metro") || lower.Contains("km"))
        {
            return ("DISTANCIA", "km", "decimal", "cardio");
        }

        var normalizedKey = normalizedName.Trim().ToUpperInvariant().Replace(' ', '_');
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            normalizedKey = "MEASUREMENT_TYPE";
        }

        return (normalizedKey, "count", "integer", "general");
    }
}