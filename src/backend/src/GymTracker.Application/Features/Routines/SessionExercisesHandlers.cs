using GymTracker.Application.Contracts.Routines;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Routines;

namespace GymTracker.Application.Features.Routines;

public sealed class SessionExercisesHandlers
{
    private readonly IRoutineRepository _routineRepository;

    public SessionExercisesHandlers(IRoutineRepository routineRepository)
    {
        _routineRepository = routineRepository;
    }

    public async Task<SessionExerciseDto?> AddAsync(string userId, string routineId, string sessionId, AddSessionExerciseDto dto, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        var session = routine?.Sessions.FirstOrDefault(item => item.Id == sessionId && !item.IsDeleted);
        if (routine is null || session is null || routine.IsDeleted)
        {
            return null;
        }

        var exercise = new SessionExerciseLink
        {
            RoutineId = routineId,
            SessionId = sessionId,
            ExerciseId = dto.ExerciseId,
            Name = dto.Name,
            PlannedSets =
            {
                new PlannedSet(Guid.NewGuid().ToString("N"), 8, 20, 1),
            },
        };

        session.Exercises.Add(exercise);
        routine.Update(routine.Title, routine.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);

        return new SessionExerciseDto(
            exercise.Id,
            exercise.ExerciseId,
            exercise.Name,
            exercise.PlannedSets.Select(setItem => new PlannedSetDto(setItem.Id, setItem.Repetitions, setItem.WeightKg, setItem.Order)).ToArray());
    }

    public async Task<bool> UnlinkAsync(string userId, string routineId, string sessionId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        var session = routine?.Sessions.FirstOrDefault(item => item.Id == sessionId && !item.IsDeleted);
        if (routine is null || session is null)
        {
            return false;
        }

        var exercise = session.Exercises.FirstOrDefault(item => item.Id == exerciseId && !item.IsDeleted);
        if (exercise is null)
        {
            return false;
        }

        exercise.Unlink();
        routine.Update(routine.Title, routine.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return true;
    }
}
