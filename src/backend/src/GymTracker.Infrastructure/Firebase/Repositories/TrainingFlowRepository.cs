using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Training;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase.Repositories;

public sealed class TrainingFlowRepository : ITrainingFlowRepository
{
    private const string FlowScope = "training-flow-v2";
    private const string LogsModule = "exercise-training-logs-v2";

    private readonly SqlDocumentStore _store;

    public TrainingFlowRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task<TrainingFlowState?> GetActiveStateAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.GetDocumentAsync<TrainingFlowState>(FlowScope, userId, cancellationToken);
    }

    public Task SaveActiveStateAsync(TrainingFlowState state, CancellationToken cancellationToken = default)
    {
        return _store.UpsertDocumentAsync(FlowScope, state.UserId, state, cancellationToken);
    }

    public Task ClearActiveStateAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.UpsertDocumentAsync<TrainingFlowState?>(FlowScope, userId, null, cancellationToken);
    }

    public Task SaveLogAsync(ExerciseTrainingLog log, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(LogsModule, log.UserId, log.Id, log, cancellationToken);
    }

    public Task<ExerciseTrainingLog?> GetLogByIdAsync(string userId, string logId, CancellationToken cancellationToken = default)
    {
        return _store.GetUserAsync<ExerciseTrainingLog>(LogsModule, userId, logId, cancellationToken);
    }

    public Task<IReadOnlyCollection<ExerciseTrainingLog>> ListLogsByExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        return _store.ListUserAsync<ExerciseTrainingLog>(LogsModule, userId, cancellationToken)
            .ContinueWith(task =>
            {
                var logs = task.Result;
                return (IReadOnlyCollection<ExerciseTrainingLog>)logs
                    .Where(log => string.Equals(log.ExerciseId, exerciseId, StringComparison.Ordinal))
                    .OrderByDescending(log => log.CreatedAt)
                    .ToArray();
            }, cancellationToken);
    }
}
