using FluentAssertions;

using GymTracker.Application.Features.Training;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.UnitTests.Training;

public sealed class TrainingFlowStateRulesTests
{
    [Fact]
    public async Task StartAndCancel_ShouldManageLockState()
    {
        var repository = new InMemoryTrainingFlowRepository();
        var handlers = new TrainingFlowHandlers(repository);

        var started = await handlers.StartAsync("user-1", new("routine-1"));
        started.IsLocked.Should().BeTrue();

        await handlers.CancelAsync("user-1");
        var active = await handlers.GetActiveAsync("user-1");
        active.Should().BeNull();
    }

    private sealed class InMemoryTrainingFlowRepository : ITrainingFlowRepository
    {
        private readonly Dictionary<string, TrainingFlowState> _states = new();

        public Task<TrainingFlowState?> GetActiveStateAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_states.TryGetValue(userId, out var state) ? state : null);

        public Task SaveActiveStateAsync(TrainingFlowState state, CancellationToken cancellationToken = default)
        {
            _states[state.UserId] = state;
            return Task.CompletedTask;
        }

        public Task ClearActiveStateAsync(string userId, CancellationToken cancellationToken = default)
        {
            _states.Remove(userId);
            return Task.CompletedTask;
        }

        public Task SaveLogAsync(ExerciseTrainingLog log, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<ExerciseTrainingLog?> GetLogByIdAsync(string userId, string logId, CancellationToken cancellationToken = default)
            => Task.FromResult<ExerciseTrainingLog?>(null);

        public Task<IReadOnlyCollection<ExerciseTrainingLog>> ListLogsByExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<ExerciseTrainingLog>>(Array.Empty<ExerciseTrainingLog>());
    }
}
