using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.MuscleGroups;

public interface IMuscleGroupRepository
{
    Task<MuscleGroup?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MuscleGroup>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(MuscleGroup entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default);
}
