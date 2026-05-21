namespace GymTracker.Application.Admin.MeasurementTypes;

public sealed class MeasurementTypeService
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "category",
        "key",
        "description",
    };

    private static readonly HashSet<string> AllowedSortDirection = new(StringComparer.OrdinalIgnoreCase)
    {
        "asc",
        "desc",
    };

    private static readonly HashSet<string> AllowedDataTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "integer",
        "decimal",
        "time",
        "boolean",
        "text",
    };

    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "strength",
        "cardio",
        "mobility",
        "general",
    };

    private readonly IMeasurementTypeRepository _repository;

    public MeasurementTypeService(IMeasurementTypeRepository repository)
    {
        _repository = repository;
    }

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
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        return _repository.ListPageAsync(
            includeInactive,
            search,
            code,
            normalizedSortBy,
            normalizedSortDirection,
            normalizedPage,
            normalizedPageSize,
            cancellationToken);
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

    public Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
    {
        return _repository.ListAssignableAsync(cancellationToken);
    }

    public Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        return _repository.CreateAsync(request, cancellationToken);
    }

    public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        return _repository.UpdateAsync(id, request, cancellationToken);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        return _repository.ReactivateAsync(id, cancellationToken);
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
                var upsert = new UpsertMeasurementTypeRequest
                {
                    Key = row.Key,
                    Name = row.Name,
                    Unit = row.Unit,
                    DataType = row.DataType,
                    Category = row.Category,
                    Description = row.Description,
                    Active = true,
                };

                Validate(upsert);
                var created = await _repository.CreateAsync(upsert, cancellationToken);
                createdRows++;
                results.Add(new ImportMeasurementTypeRowResult(rowNumber, created.Key, true, null, created));
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                var rowKey = string.IsNullOrWhiteSpace(row.Key) ? null : row.Key.Trim();
                results.Add(new ImportMeasurementTypeRowResult(rowNumber, rowKey, false, exception.Message, null));
            }

            rowNumber++;
        }

        return new ImportMeasurementTypesResult(
            TotalRows: rows.Count,
            CreatedRows: createdRows,
            RejectedRows: rows.Count - createdRows,
            Rows: results);
    }

    private static void Validate(UpsertMeasurementTypeRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.DataType)
            && !AllowedDataTypes.Contains(request.DataType))
        {
            throw new ArgumentException("DataType is invalid. Allowed values: integer, decimal, time, boolean, text.", nameof(request.DataType));
        }

        if (!string.IsNullOrWhiteSpace(request.Category)
            && !AllowedCategories.Contains(request.Category))
        {
            throw new ArgumentException("Category is invalid. Allowed values: strength, cardio, mobility, general.", nameof(request.Category));
        }
    }
}
