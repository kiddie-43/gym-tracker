namespace GymTracker.Application.Units;

public sealed record UpsertUnitsRequest(
    string Name,
    string Description);
    public sealed record CreateUnitsRequest(
    string Code,
    string Name,
    string Description);
    
public sealed record UnitsResponse(
    Guid Id,
    string Code,
    string Name,
    string Description
    );

public sealed record UnitsPageResponse(
    IReadOnlyCollection<UnitsResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record FilterUnitsRequest(
    string? Code,
    string? Name,
    string? Description,
    int Page = 1,
    int PageSize = 10);

public sealed record ImportUnitsRequest(
    IReadOnlyCollection<ImportUnitRowRequest> Rows);

public sealed record ImportUnitRowRequest(
    string? Name,
    string? Code,
    string? Description);

public sealed record ImportUnitsResult(
    int TotalRows,
    int ImportedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportUnitRowResult> Results);

public sealed record ImportUnitRowResult(
    int RowNumber,
    string? Code,
    bool Imported,
    string? Reason,
    UnitsResponse? Unit);