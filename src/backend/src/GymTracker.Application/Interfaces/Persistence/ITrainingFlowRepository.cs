using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.Interfaces.Persistence;

public interface ITrainingFlowRepository
{
    Task<TrainingFlowState?> GetActiveStateAsync(string userId, CancellationToken cancellationToken = default);

    Task SaveActiveStateAsync(TrainingFlowState state, CancellationToken cancellationToken = default);

    Task ClearActiveStateAsync(string userId, CancellationToken cancellationToken = default);

    Task SaveLogAsync(ExerciseTrainingLog log, CancellationToken cancellationToken = default);

    Task<ExerciseTrainingLog?> GetLogByIdAsync(string userId, string logId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExerciseTrainingLog>> ListLogsByExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default);
}
