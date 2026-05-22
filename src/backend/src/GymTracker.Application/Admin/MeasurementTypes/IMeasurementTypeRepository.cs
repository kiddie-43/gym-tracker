using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.MeasurementTypes;

public interface IMeasurementTypeRepository
{
    Task<MeasurementType?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MeasurementType>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveCodeAsync(string code, string? excludeId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(MeasurementType entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default);
}

