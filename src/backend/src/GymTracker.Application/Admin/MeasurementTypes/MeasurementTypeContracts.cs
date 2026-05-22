namespace GymTracker.Application.Admin.MeasurementTypes;

public sealed record UpsertMeasurementTypeRequest
{
    public string? Code { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}

public sealed record MeasurementTypeResponse(
    string Id,
    string Code,
    string Name,
    string? Description,
    bool IsDeleted);

public sealed record AssignableMeasurementTypeResponse(
    string Id,
    string Code,
    string Name,
    string? Description);

public sealed record MeasurementTypesPageResponse(
    IReadOnlyCollection<MeasurementTypeResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ImportMeasurementTypesRequest(
    IReadOnlyCollection<ImportMeasurementTypeRowRequest> Rows);

public sealed record ImportMeasurementTypeRowRequest(
    string? Code,
    string? Name,
    string? Description);

public sealed record ImportMeasurementTypesResult(
    int TotalRows,
    int CreatedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportMeasurementTypeRowResult> Rows);

public sealed record ImportMeasurementTypeRowResult(
    int RowNumber,
    string? Code,
    bool Created,
    string? Reason,
    MeasurementTypeResponse? MeasurementType);
