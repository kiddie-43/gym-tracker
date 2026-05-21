using GymTracker.Application.Contracts.Routines;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Validation;
using GymTracker.Domain.Entities.Routines;

namespace GymTracker.Application.Features.Routines;

public sealed class RoutinesCommandHandlers
{
    private readonly IRoutineRepository _routineRepository;

    public RoutinesCommandHandlers(IRoutineRepository routineRepository)
    {
        _routineRepository = routineRepository;
    }

    public async Task<IReadOnlyCollection<RoutineCardDto>> ListAsync(string userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var routines = await _routineRepository.ListByUserAsync(userId, cancellationToken);

        var filteredRoutines = includeDeleted
            ? routines
            : routines.Where(routine => !routine.IsDeleted);

        return filteredRoutines
            .OrderByDescending(routine => routine.CreatedAt)
            .Select(ToCard)
            .ToArray();
    }

    public async Task<RoutineDetailDto> CreateAsync(string userId, CreateRoutineDto dto, CancellationToken cancellationToken = default)
    {
        RoutinesValidationRules.EnsureRoutineTitle(dto.Title);

        var routine = new Routine
        {
            UserId = userId,
            Title = dto.Title.Trim(),
            Goal = dto.Goal?.Trim(),
        };

        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return ToDetail(routine);
    }

    public async Task<RoutineDetailDto?> UpdateAsync(string userId, string routineId, UpdateRoutineDto dto, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (routine is null)
        {
            return null;
        }

        var title = dto.Title?.Trim();
        if (!string.IsNullOrWhiteSpace(title))
        {
            RoutinesValidationRules.EnsureRoutineTitle(title);
        }

        ApplyLastWriteWins(routine, title, dto.Goal);
        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return ToDetail(routine);
    }

    private static void ApplyLastWriteWins(Routine routine, string? title, string? goal)
    {
        // Last-write-wins: the latest accepted update request overwrites prior values.
        routine.Update(title ?? routine.Title, goal?.Trim() ?? routine.Goal);
    }

    public async Task<bool> ArchiveAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (routine is null)
        {
            return false;
        }

        routine.Archive();
        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return true;
    }

    public async Task<RoutineDetailDto?> ReactivateAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (routine is null)
        {
            return null;
        }

        routine.Reactivate();
        await _routineRepository.UpsertAsync(routine, cancellationToken);
        return ToDetail(routine);
    }

    public async Task<RoutineDetailDto?> GetByIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (routine is null)
        {
            return null;
        }

        return ToDetail(routine);
    }

    private static RoutineCardDto ToCard(Routine routine)
    {
        return new RoutineCardDto(
            routine.Id,
            routine.Title,
            routine.CreatedAt,
            routine.Sessions.Where(session => !session.IsDeleted).SelectMany(session => session.Exercises).Count(exercise => !exercise.IsDeleted),
            routine.IsDeleted);
    }

    internal static RoutineDetailDto ToDetail(Routine routine)
    {
        var sessions = routine.Sessions
            .Where(session => !session.IsDeleted)
            .Select(session => new RoutineSessionDto(
                session.Id,
                session.Name,
                session.DaysOfWeek,
                session.Exercises
                    .Where(exercise => !exercise.IsDeleted)
                    .Select(exercise => new SessionExerciseDto(
                        exercise.Id,
                        exercise.ExerciseId,
                        exercise.Name,
                        exercise.PlannedSets
                            .OrderBy(setItem => setItem.Order)
                            .Select(setItem => new PlannedSetDto(setItem.Id, setItem.Repetitions, setItem.WeightKg, setItem.Order))
                            .ToArray()))
                    .ToArray()))
            .ToArray();

        return new RoutineDetailDto(
            routine.Id,
            routine.Title,
            routine.Goal,
            routine.CreatedAt,
            routine.IsDeleted,
            sessions,
            sessions.SelectMany(session => session.Exercises).Count());
    }
}
