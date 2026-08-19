namespace GymTracker.Application.Common.Importing;

public sealed record ImportResult<TResponse>(
    int TotalRows,
    int ImportedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportRowResult<TResponse>> Results);