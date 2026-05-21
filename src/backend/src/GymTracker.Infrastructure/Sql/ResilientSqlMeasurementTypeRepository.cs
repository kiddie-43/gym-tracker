using System.Threading;

using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Infrastructure.Firebase;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace GymTracker.Infrastructure.Sql;

public sealed class ResilientSqlMeasurementTypeRepository : IMeasurementTypeRepository
{
    private static int _fallbackMode;

    private readonly SqlMeasurementTypeRepository _primary;
    private readonly InMemoryMeasurementTypeRepository _fallback;
    private readonly ILogger<ResilientSqlMeasurementTypeRepository> _logger;

    public ResilientSqlMeasurementTypeRepository(
        SqlMeasurementTypeRepository primary,
        InMemoryMeasurementTypeRepository fallback,
        ILogger<ResilientSqlMeasurementTypeRepository> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public Task<MeasurementTypesPageResponse> ListPageAsync(
        bool includeInactive = false,
        string? search = null,
        string? code = null,
        string sortBy = "name",
        string sortDirection = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ExecuteWithFallback(
            () => _primary.ListPageAsync(includeInactive, search, code, sortBy, sortDirection, page, pageSize, cancellationToken),
            () => _fallback.ListPageAsync(includeInactive, search, code, sortBy, sortDirection, page, pageSize, cancellationToken));

    public Task<IReadOnlyCollection<AssignableMeasurementTypeResponse>> ListAssignableAsync(CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.ListAssignableAsync(cancellationToken), () => _fallback.ListAssignableAsync(cancellationToken));

    public Task<MeasurementTypeResponse?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.GetByIdAsync(id, cancellationToken), () => _fallback.GetByIdAsync(id, cancellationToken));

    public Task<MeasurementTypeResponse> CreateAsync(UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.CreateAsync(request, cancellationToken), () => _fallback.CreateAsync(request, cancellationToken));

    public Task<MeasurementTypeResponse?> UpdateAsync(string id, UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.UpdateAsync(id, request, cancellationToken), () => _fallback.UpdateAsync(id, request, cancellationToken));

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        => ExecuteWithFallback(() => _primary.DeleteAsync(id, cancellationToken), () => _fallback.DeleteAsync(id, cancellationToken));

    public Task<MeasurementTypeResponse?> ReactivateAsync(string id, CancellationToken cancellationToken = default)
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
            _logger.LogWarning(exception, "SQL connection failed for measurement types. Switching to in-memory fallback until application restart.");
            return await fallbackCall();
        }
    }
}
