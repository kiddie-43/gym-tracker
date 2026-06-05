namespace GymTracker.Application.Admin.Muscles;

public sealed record CreateMuscleRequest(
    string Code,
    string Name,
    string? Description);

public sealed record UpdateMuscleRequest(
    string Name,
    string? Description);

public sealed record MuscleResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description
    );

public sealed record MusclesPageResponse(
    IReadOnlyCollection<MuscleResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ImportMusclesRequest(
    IReadOnlyCollection<ImportMuscleRowRequest> Rows);

public sealed record ImportMuscleRowRequest(
    string? Name,
    string? Code,
    string? Description);

public sealed record ImportMusclesResult(
    int TotalRows,
    int ImportedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportMuscleRowResult> Results);

public sealed record ImportMuscleRowResult(
    int RowNumber,
    string? Code,
    bool Imported,
    string? Reason,
    MuscleResponse? Muscle);
