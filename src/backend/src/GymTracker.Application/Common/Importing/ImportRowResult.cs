namespace GymTracker.Application.Common.Importing;

public sealed record ImportRowResult<TResponse>(
    int RowNumber,
    string? Code,
    bool Success,
    string? Error,
    TResponse? Entity);