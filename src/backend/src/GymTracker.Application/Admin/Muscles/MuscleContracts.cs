namespace GymTracker.Application.Admin.Muscles;

public sealed record UpsertMuscleRequest(
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<string> MuscleGroupIds,
    bool Active = true);

public sealed record MuscleResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<string> MuscleGroupIds,
    bool Active,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

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
