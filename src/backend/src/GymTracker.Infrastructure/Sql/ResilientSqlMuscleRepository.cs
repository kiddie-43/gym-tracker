using System.Threading;

using GymTracker.Application.Admin.Muscles;
using GymTracker.Domain.Entities;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GymTracker.Infrastructure.Sql;

public sealed class ResilientSqlMuscleRepository : IMuscleRepository
{
    private static int _fallbackMode;

    private readonly SqlMuscleRepository _primary;
    private readonly MuscleRepository _fallback;
    private readonly ILogger<ResilientSqlMuscleRepository> _logger;

    public ResilientSqlMuscleRepository(
        SqlMuscleRepository primary,
        MuscleRepository fallback,
        ILogger<ResilientSqlMuscleRepository> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public Task<Muscle?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.GetByIdAsync(id, cancellationToken), () => _fallback.GetByIdAsync(id, cancellationToken));

    public Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.ListAsync(includeDeleted, cancellationToken), () => _fallback.ListAsync(includeDeleted, cancellationToken));

    public Task<bool> ExistsActiveCodeAsync(string code, string? excludingId = null, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(
            () => _primary.ExistsActiveCodeAsync(code, excludingId, cancellationToken),
            () => _fallback.ExistsActiveCodeAsync(code, excludingId, cancellationToken));

    public Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.SaveAsync(entity, cancellationToken), () => _fallback.SaveAsync(entity, cancellationToken));

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
            _logger.LogWarning(exception, "SQL connection failed for muscles. Switching to in-memory fallback until application restart.");
            return await fallbackCall();
        }
    }

    private async Task ExecuteWithFallback(Func<Task> primaryCall, Func<Task> fallbackCall)
    {
        if (Volatile.Read(ref _fallbackMode) == 1)
        {
            await fallbackCall();
            return;
        }

        try
        {
            await primaryCall();
        }
        catch (SqlException exception)
        {
            Interlocked.Exchange(ref _fallbackMode, 1);
            _logger.LogWarning(exception, "SQL connection failed for muscles. Switching to in-memory fallback until application restart.");
            await fallbackCall();
        }
    }
}
