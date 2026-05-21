using System.Threading;

using GymTracker.Application.Admin.Exercises;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GymTracker.Infrastructure.Sql;

public sealed class ResilientSqlExerciseRepository : IExerciseRepository
{
    private static int _fallbackMode;

    private readonly SqlExerciseRepository _primary;
    private readonly ExerciseRepository _fallback;
    private readonly ILogger<ResilientSqlExerciseRepository> _logger;

    public ResilientSqlExerciseRepository(
        SqlExerciseRepository primary,
        ExerciseRepository fallback,
        ILogger<ResilientSqlExerciseRepository> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public Task<Domain.Entities.Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.GetByIdAsync(id, cancellationToken), () => _fallback.GetByIdAsync(id, cancellationToken));

    public Task<IReadOnlyCollection<Domain.Entities.Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.ListAsync(includeDeleted, cancellationToken), () => _fallback.ListAsync(includeDeleted, cancellationToken));

    public Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(
            () => _primary.ExistsActiveCodeAsync(code, excludeExerciseId, cancellationToken),
            () => _fallback.ExistsActiveCodeAsync(code, excludeExerciseId, cancellationToken));

    public Task SaveAsync(Domain.Entities.Exercise entity, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(
            async () =>
            {
                await _primary.SaveAsync(entity, cancellationToken);
                return true;
            },
            async () =>
            {
                await _fallback.SaveAsync(entity, cancellationToken);
                return true;
            });

    public Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.DeleteAsync(id, now, cancellationToken), () => _fallback.DeleteAsync(id, now, cancellationToken));

    public Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.ReactivateAsync(id, cancellationToken), () => _fallback.ReactivateAsync(id, cancellationToken));

    private async Task<T> ExecuteWithFallback<T>(Func<Task<T>> primaryCall, Func<Task<T>> fallbackCall)
    {
        if (Volatile.Read(ref _fallbackMode) == 1)
        {
            return await fallbackCall();
        }

        try
        {
            return await primaryCall();
        }
        catch (SqlException exception)
        {
            Interlocked.Exchange(ref _fallbackMode, 1);
            _logger.LogWarning(exception, "SQL connection failed for exercises. Switching to in-memory fallback until application restart.");
            return await fallbackCall();
        }
    }
}
