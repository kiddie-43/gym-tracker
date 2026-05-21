using GymTracker.Application.Contracts.Training;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Training;

namespace GymTracker.Application.Features.Training;

public sealed class TrainingFlowHandlers
{
    private readonly ITrainingFlowRepository _repository;

    public TrainingFlowHandlers(ITrainingFlowRepository repository)
    {
        _repository = repository;
    }

    public async Task<TrainingFlowStateDto> StartAsync(string userId, StartTrainingFlowDto dto, CancellationToken cancellationToken = default)
    {
        var state = new TrainingFlowState
        {
            UserId = userId,
            IsLocked = true,
            RoutineId = dto.RoutineId,
            StepNode = "routine",
            LastUpdatedAt = DateTimeOffset.UtcNow,
        };

        await _repository.SaveActiveStateAsync(state, cancellationToken);
        return ToDto(state);
    }

    public async Task CancelAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _repository.ClearActiveStateAsync(userId, cancellationToken);
    }

    public async Task<TrainingFlowStateDto?> GetActiveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var state = await _repository.GetActiveStateAsync(userId, cancellationToken);
        if (state is null || !state.IsLocked)
        {
            return null;
        }

        return ToDto(state);
    }

    public async Task<TrainingFlowStateDto?> UpdateAsync(string userId, UpdateTrainingFlowDto dto, CancellationToken cancellationToken = default)
    {
        var state = await _repository.GetActiveStateAsync(userId, cancellationToken);
        if (state is null || !state.IsLocked)
        {
            return null;
        }

        RoutinesValidationRules.EnsureTrainingStepNode(dto.StepNode);

        state.StepNode = dto.StepNode;
        state.RoutineId = dto.RoutineId ?? state.RoutineId;
        state.SessionId = dto.SessionId;
        state.ExerciseId = dto.ExerciseId;
        state.LastUpdatedAt = DateTimeOffset.UtcNow;

        await _repository.SaveActiveStateAsync(state, cancellationToken);
        return ToDto(state);
    }

    private static TrainingFlowStateDto ToDto(TrainingFlowState state)
    {
        return new TrainingFlowStateDto(state.IsLocked, state.RoutineId, state.SessionId, state.ExerciseId, state.StepNode, state.LastUpdatedAt);
    }
}
