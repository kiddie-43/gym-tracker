namespace GymTracker.Application.Admin.MeasurementTypes;

public sealed record MeasurementFieldRequest(string Name);

public sealed record UpsertMeasurementTypeRequest
{
    public string? Key { get; init; }

    public string? Name { get; init; }

    public string? Unit { get; init; }

    public string? DataType { get; init; }

    public string? Category { get; init; }

    public string? Description { get; init; }

    public IReadOnlyCollection<MeasurementFieldRequest>? Fields { get; init; }

    public bool Active { get; init; } = true;
}

public sealed record MeasurementTypeResponse(
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

public sealed record AssignableMeasurementTypeResponse(
    string Id,
    string Key,
    string Name,
    string Unit,
    string DataType,
    string Category);

public sealed record MeasurementTypesPageResponse(
    IReadOnlyCollection<MeasurementTypeResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ImportMeasurementTypesRequest(
    IReadOnlyCollection<ImportMeasurementTypeRowRequest> Rows);

public sealed record ImportMeasurementTypeRowRequest(
    string? Key,
    string? Name,
    string? Unit,
    string? DataType,
    string? Category,
    string? Description);

public sealed record ImportMeasurementTypesResult(
    int TotalRows,
    int CreatedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportMeasurementTypeRowResult> Rows);

public sealed record ImportMeasurementTypeRowResult(
    int RowNumber,
    string? Key,
    bool Created,
    string? Reason,
    MeasurementTypeResponse? MeasurementType);
