using GymTracker.Application.Contracts.Routines;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Validation;

namespace GymTracker.Application.Features.Routines;

public sealed class PlannedSetsHandlers
{
    private readonly IRoutineRepository _routineRepository;

    public PlannedSetsHandlers(IRoutineRepository routineRepository)
    {
        _routineRepository = routineRepository;
    }

    public async Task<PlannedSetDto?> UpdateAsync(
        string userId,
        string routineId,
        string sessionId,
        string exerciseId,
        string setId,
        UpdatePlannedSetDto dto,
        CancellationToken cancellationToken = default)
    {
        RoutinesValidationRules.EnsurePlannedSet(dto.Repetitions, dto.WeightKg);

        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        var setItem = routine?.Sessions
            .Where(session => !session.IsDeleted && session.Id == sessionId)
            .SelectMany(session => session.Exercises)
            .Where(exercise => !exercise.IsDeleted && exercise.Id == exerciseId)
            .SelectMany(exercise => exercise.PlannedSets)
            .FirstOrDefault(setEntry => setEntry.Id == setId);

        if (routine is null || setItem is null)
        {
            return null;
        }

        setItem.Update(dto.Repetitions, dto.WeightKg);
        routine.Update(routine.Title, routine.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);

        return new PlannedSetDto(setItem.Id, setItem.Repetitions, setItem.WeightKg, setItem.Order);
    }

    public async Task<bool> DeleteAsync(string userId, string routineId, string sessionId, string exerciseId, string setId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        var exercise = routine?.Sessions
            .Where(session => !session.IsDeleted && session.Id == sessionId)
            .SelectMany(session => session.Exercises)
            .FirstOrDefault(item => !item.IsDeleted && item.Id == exerciseId);

        if (routine is null || exercise is null)
        {
            return false;
        }

        var removed = exercise.PlannedSets.RemoveAll(setItem => setItem.Id == setId) > 0;
        if (!removed)
        {
            return false;
        }

        routine.Update(routine.Title, routine.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return true;
    }
}
