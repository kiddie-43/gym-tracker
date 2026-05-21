using System.Globalization;
using System.Text;

using GymTracker.Application.Admin.Common;
using GymTracker.Application.Admin.MeasurementTypes;

namespace GymTracker.Infrastructure.Firebase;

public sealed class MeasurementTypeRepository : IMeasurementTypeRepository
{
    private readonly IAdminDocumentStore _store;

    private const string CollectionName = FirestoreContext.AdminMeasurementTypesCollection;

    public MeasurementTypeRepository(IAdminDocumentStore store)
    {
        _store = store;
    }

    public async Task<MeasurementTypesPageResponse> ListPageAsync(
        bool includeInactive = false,
        string? search = null,
        string? code = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var rows = await _store.QueryAdminCollectionAsync<MeasurementTypeDocument>(CollectionName, cancellationToken);

        var filtered = rows
            .Select(Map)
            .Where(item => includeInactive || !item.IsDeleted)
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = NormalizeForSearch(search);
            filtered = filtered
                .Where(item =>
                    NormalizeForSearch(item.Key).Contains(normalizedSearch) ||
                    NormalizeForSearch(item.Name).Contains(normalizedSearch) ||
                    NormalizeForSearch(item.Category).Contains(normalizedSearch) ||
                    NormalizeForSearch(item.Description ?? string.Empty).Contains(normalizedSearch))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            var normalizedCode = NormalizeForSearch(code);
            filtered = filtered
                .Where(item => NormalizeForSearch(item.Key).Contains(normalizedCode))
                .ToList();
        }

        var comparer = StringComparer.OrdinalIgnoreCase;
        IOrderedEnumerable<MeasurementTypeResponse> ordered = sortBy.ToLowerInvariant() switch
        {
            "category" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filtered.OrderByDescending(item => item.Category, comparer).ThenBy(item => item.Id, comparer)
                : filtered.OrderBy(item => item.Category, comparer).ThenBy(item => item.Id, comparer),
            "description" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filtered.OrderByDescending(item => item.Description ?? string.Empty, comparer).ThenBy(item => item.Id, comparer)
                : filtered.OrderBy(item => item.Description ?? string.Empty, comparer).ThenBy(item => item.Id, comparer),
            "code" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filtered.OrderByDescending(item => item.Key, comparer).ThenBy(item => item.Id, comparer)
                : filtered.OrderBy(item => item.Key, comparer).ThenBy(item => item.Id, comparer),
            "key" => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filtered.OrderByDescending(item => item.Key, comparer).ThenBy(item => item.Id, comparer)
                : filtered.OrderBy(item => item.Key, comparer).ThenBy(item => item.Id, comparer),
            _ => sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? filtered.OrderByDescending(item => item.Name, comparer).ThenBy(item => item.Id, comparer)
                : filtered.OrderBy(item => item.Name, comparer).ThenBy(item => item.Id, comparer),
        };

        var totalCount = filtered.Count;
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;
        var skip = (normalizedPage - 1) * normalizedPageSize;
        var pagedItems = ordered.Skip(skip).Take(normalizedPageSize).ToArray();

        return new MeasurementTypesPageResponse(pagedItems, totalCount, normalizedPage, normalizedPageSize);
    }

    public async Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _store.QueryAdminCollectionAsync<MeasurementTypeDocument>(CollectionName, cancellationToken);

        return rows
            .Select(Map)
            .Where(item => !item.IsDeleted)
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
            .Select(item => new AssignableMeasurementTypeResponse(
                item.Id,
                item.Key,
                item.Name,
                item.Unit,
                item.DataType,
                item.Category))
            .ToArray();
    }

    public async Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var key = _store.BuildAdminKey(CollectionName, id);
        var row = await _store.GetAsync<MeasurementTypeDocument>(key, cancellationToken);

        if (row is null || row.IsDeleted)
        {
            return null;
        }

        return Map(row);
    }

    public async Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = SanitizeRequest(request);
        var key = normalized.Key!;
        var name = normalized.Name!;
        var unit = normalized.Unit!;
        var dataType = normalized.DataType!;
        var category = normalized.Category!;

        var hasConflict = await HasActiveKeyConflictAsync(key, excludingId: null, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var now = DateTimeOffset.UtcNow;
        var row = new MeasurementTypeDocument(
            Id: Guid.NewGuid().ToString("N"),
            Key: key,
            Name: name,
            Unit: unit,
            DataType: dataType,
            Category: category,
            Description: normalized.Description,
            Active: normalized.Active,
            IsDeleted: false,
            CreatedAt: now,
            UpdatedAt: now,
            DeletedAt: null);

        await SaveAsync(row, cancellationToken);
        return Map(row);
    }

    public async Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        var key = _store.BuildAdminKey(CollectionName, id);
        var row = await _store.GetAsync<MeasurementTypeDocument>(key, cancellationToken);
        if (row is null || row.IsDeleted)
        {
            return null;
        }

        var normalized = SanitizeRequest(request);
        var normalizedKey = normalized.Key!;
        var name = normalized.Name!;
        var unit = normalized.Unit!;
        var dataType = normalized.DataType!;
        var category = normalized.Category!;

        var hasConflict = await HasActiveKeyConflictAsync(normalizedKey, excludingId: row.Id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = row with
        {
            Key = normalizedKey,
            Name = name,
            Unit = unit,
            DataType = dataType,
            Category = category,
            Description = normalized.Description,
            Active = normalized.Active,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await SaveAsync(updated, cancellationToken);
        return Map(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var key = _store.BuildAdminKey(CollectionName, id);
        var row = await _store.GetAsync<MeasurementTypeDocument>(key, cancellationToken);
        if (row is null)
        {
            return false;
        }

        var updated = row with
        {
            Active = false,
            IsDeleted = true,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = DateTimeOffset.UtcNow,
        };

        await SaveAsync(updated, cancellationToken);
        return true;
    }

    public async Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var key = _store.BuildAdminKey(CollectionName, id);
        var row = await _store.GetAsync<MeasurementTypeDocument>(key, cancellationToken);
        if (row is null)
        {
            return null;
        }

        var hasConflict = await HasActiveKeyConflictAsync(row.Key, row.Id, cancellationToken);
        if (hasConflict)
        {
            throw new InvalidOperationException("Key already exists among active records.");
        }

        var updated = row with
        {
            Active = true,
            IsDeleted = false,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = null,
        };

        await SaveAsync(updated, cancellationToken);
        return Map(updated);
    }

    private async Task SaveAsync(MeasurementTypeDocument row, CancellationToken cancellationToken)
    {
        var entityKey = _store.BuildAdminKey(CollectionName, row.Id);
        var codeKey = _store.BuildAdminIndexKey(CollectionName, row.Key);

        await _store.SetAsync(entityKey, row, cancellationToken);
        await _store.SetAsync(codeKey, row.Id, cancellationToken);
    }

    private async Task<bool> HasActiveKeyConflictAsync(string key, string? excludingId, CancellationToken cancellationToken)
    {
        var rows = await _store.QueryAdminCollectionAsync<MeasurementTypeDocument>(CollectionName, cancellationToken);

        return rows.Any(item =>
            !item.IsDeleted
            && string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(item.Id, excludingId, StringComparison.OrdinalIgnoreCase));
    }

    private static UpsertMeasurementTypeRequest SanitizeRequest(UpsertMeasurementTypeRequest request)
    {
        var normalizedName = AdminRequestSanitizer.RequiredTrimmed(request.Name, nameof(request.Name));
        var inferred = InferDefaults(normalizedName);
        var normalizedKey = string.IsNullOrWhiteSpace(request.Key)
            ? inferred.Key
            : AdminRequestSanitizer.RequiredUpperCode(request.Key, nameof(request.Key));

        var legacyHasFields = request.Fields is { Count: > 0 };
        var normalizedUnit = string.IsNullOrWhiteSpace(request.Unit)
            ? (legacyHasFields ? "count" : inferred.Unit)
            : AdminRequestSanitizer.RequiredTrimmed(request.Unit, nameof(request.Unit));

        var normalizedDataType = string.IsNullOrWhiteSpace(request.DataType)
            ? (legacyHasFields ? "integer" : inferred.DataType)
            : AdminRequestSanitizer.RequiredTrimmed(request.DataType, nameof(request.DataType)).ToLowerInvariant();

        var normalizedCategory = string.IsNullOrWhiteSpace(request.Category)
            ? (legacyHasFields ? "general" : inferred.Category)
            : AdminRequestSanitizer.RequiredTrimmed(request.Category, nameof(request.Category)).ToLowerInvariant();

        return request with
        {
            Key = normalizedKey,
            Name = normalizedName,
            Unit = normalizedUnit,
            DataType = normalizedDataType,
            Category = normalizedCategory,
            Description = AdminRequestSanitizer.OptionalTrimmed(request.Description),
        };
    }

    private static string NormalizeCode(string value)
    {
        var upper = value.Trim().ToUpperInvariant();
        var normalized = upper.Replace(' ', '_');
        return string.IsNullOrWhiteSpace(normalized) ? "MEASUREMENT_TYPE" : normalized;
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

        return (NormalizeCode(normalizedName), "count", "integer", "general");
    }

    private static string NormalizeForSearch(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var nfdText = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in nfdText)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return sb.ToString().ToLowerInvariant();
    }

    private static MeasurementTypeResponse Map(MeasurementTypeDocument row)
    {
        return new MeasurementTypeResponse(
            row.Id,
            row.Key,
            row.Name,
            row.Unit,
            row.DataType,
            row.Category,
            row.Description,
            row.Active,
            row.IsDeleted,
            row.CreatedAt,
            row.UpdatedAt,
            row.DeletedAt);
    }

    private sealed record MeasurementTypeDocument(
        string Id,
        string Key,
        string Name,
        string Unit,
        string DataType,
        string Category,
        string? Description,
        bool Active,
        bool IsDeleted,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? DeletedAt);
}
