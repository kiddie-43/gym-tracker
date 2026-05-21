using GymTracker.Domain.Entities;

namespace GymTracker.Application.Admin.ExerciseFormTypes;

public interface IExerciseFormTypeRepository
{
    Task<ExerciseFormType?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExerciseFormType>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(ExerciseFormType entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<bool> ReactivateAsync(string id, CancellationToken cancellationToken = default);
}
