public sealed record CreateExerciceRequest(
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<Guid> Units,
    IReadOnlyCollection<Guid> PrimaryMuscles,
    IReadOnlyCollection<Guid> SecondaryMuscles
    );

public sealed record UpdateExerciceRequest(
    string Name,
    string? Description,
    IReadOnlyCollection<Guid> Units,
    IReadOnlyCollection<Guid> PrimaryMuscles,
    IReadOnlyCollection<Guid> SecondaryMuscles
    );
public sealed record ExerciceReferenceResponse(
    Guid Id,
    string Name
    
    );

public sealed record ExerciceResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<ExerciceReferenceResponse> Units,
    IReadOnlyCollection<ExerciceReferenceResponse> PrimaryMuscles,
    IReadOnlyCollection<ExerciceReferenceResponse> SecondaryMuscles
    );

    public sealed record ExercicePageResponse(
    IReadOnlyCollection<ExerciceResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

    public sealed record ImportExercicesRequest(
        IReadOnlyCollection<ImportExerciceRowRequest> Rows);

    public sealed record ImportExerciceRowRequest(
        string? Name,
        string? Code,
        string? Description);

    public sealed record ImportExercicesResult(
        int TotalRows,
        int ImportedRows,
        int RejectedRows,
        IReadOnlyCollection<ImportExerciceRowResult> Results);

    public sealed record ImportExerciceRowResult(
        int RowNumber,
        string? Code,
        bool Imported,
        string? Reason,
        ExerciceResponse? Exercice);