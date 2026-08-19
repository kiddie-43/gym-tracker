using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Muscles;

public interface IMuscleRepository
{
    Task<Muscle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Muscle>> ListAsync( CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(Muscle entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
