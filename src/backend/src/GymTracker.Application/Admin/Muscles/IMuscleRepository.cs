using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Muscles;

public interface IMuscleRepository
{
    Task<Muscle?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Muscle>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveCodeAsync(string code, string? excludingId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default);
}
