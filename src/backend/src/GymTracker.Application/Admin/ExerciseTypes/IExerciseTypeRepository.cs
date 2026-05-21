using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.ExerciseTypes;

public interface IExerciseTypeRepository
{
    Task<ExerciseType?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExerciseType>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ExerciseType entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default);
}
