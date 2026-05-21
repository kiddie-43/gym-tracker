using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.Exercises;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Exercise>> ListAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveCodeAsync(string code, string? excludeExerciseId = null, CancellationToken cancellationToken = default);

    Task SaveAsync(Exercise entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default);
}
