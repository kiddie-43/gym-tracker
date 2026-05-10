using GymTracker.Application.Catalog;
using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Routines;

public sealed class RoutineService
{
    private readonly IRoutineRepository _routineRepository;
    private readonly ICatalogService _catalogService;

    public RoutineService(IRoutineRepository routineRepository, ICatalogService catalogService)
    {
        _routineRepository = routineRepository;
        _catalogService = catalogService;
    }

    public async Task<RoutineResponse> CreateAsync(string userId, CreateRoutineRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = RoutineValidators.Validate(request);
        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(string.Join(' ', validationErrors));
        }

        var routine = new Routine
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            Tags = request.Tags,
            Days = request.Days.Select(day => new RoutineDay
            {
                DayLabel = day.DayLabel,
                Exercises = day.Exercises.Select(exercise => new PlannedExercise
                {
                    ExternalExerciseId = exercise.ExternalExerciseId,
                    ExerciseName = exercise.ExerciseName,
                    MuscleGroupIds = exercise.MuscleGroupIds,
                    TargetSets = exercise.TargetSets,
                    TargetRepetitions = exercise.TargetRepetitions,
                    TargetRestSeconds = exercise.TargetRestSeconds,
                    Notes = exercise.Notes,
                    ImageUrl = exercise.ImageUrl,
                }).ToArray(),
            }).ToArray(),
        };

        await _routineRepository.AddAsync(routine, cancellationToken);

        return MapResponse(routine);
    }

    public async Task<IReadOnlyCollection<RoutineSummaryResponse>> ListAsync(string userId, CancellationToken cancellationToken = default)
    {
        var routines = await _routineRepository.ListByUserAsync(userId, cancellationToken);

        return routines
            .OrderBy(routine => routine.Name, StringComparer.OrdinalIgnoreCase)
            .Select(routine => new RoutineSummaryResponse(routine.Id, routine.Name, routine.Tags, routine.Days.Count))
            .ToArray();
    }

    public async Task<RoutineResponse?> GetByIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        return routine is null ? null : MapResponse(routine);
    }

    public async Task<RoutineResponse?> UpdateAsync(string userId, string routineId, CreateRoutineRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = RoutineValidators.Validate(request);
        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(string.Join(' ', validationErrors));
        }

        var existing = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var updatedRoutine = new Routine
        {
            Id = existing.Id,
            UserId = existing.UserId,
            CreatedAt = existing.CreatedAt,
            Name = request.Name,
            Description = request.Description,
            Tags = request.Tags,
            Days = request.Days.Select(day => new RoutineDay
            {
                DayLabel = day.DayLabel,
                Exercises = day.Exercises.Select(exercise => new PlannedExercise
                {
                    ExternalExerciseId = exercise.ExternalExerciseId,
                    ExerciseName = exercise.ExerciseName,
                    MuscleGroupIds = exercise.MuscleGroupIds,
                    TargetSets = exercise.TargetSets,
                    TargetRepetitions = exercise.TargetRepetitions,
                    TargetRestSeconds = exercise.TargetRestSeconds,
                    Notes = exercise.Notes,
                    ImageUrl = exercise.ImageUrl,
                }).ToArray(),
            }).ToArray(),
        };

        await _routineRepository.UpdateAsync(updatedRoutine, cancellationToken);
        return MapResponse(updatedRoutine);
    }

    public async Task<bool> DeleteAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        var existing = await _routineRepository.GetByUserAndIdAsync(userId, routineId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _routineRepository.DeleteAsync(userId, routineId, cancellationToken);
        return true;
    }

    public async Task<SuggestedExercisesResponse> GetSuggestedExercisesAsync(
        IReadOnlyCollection<string> muscleGroupIds,
        string? query,
        CancellationToken cancellationToken = default)
    {
        var exercises = await _catalogService.GetExercisesAsync(null, query, muscleGroupIds, cancellationToken);
        return new SuggestedExercisesResponse(exercises);
    }

    private static RoutineResponse MapResponse(Routine routine)
    {
        return new RoutineResponse(
            routine.Id,
            routine.Name,
            routine.Description,
            routine.Tags,
            routine.Days.Select(day =>
                new RoutineDayResponse(
                    day.DayLabel,
                    day.Exercises.Select(exercise =>
                        new PlannedExerciseResponse(
                            exercise.ExternalExerciseId,
                            exercise.ExerciseName,
                            exercise.MuscleGroupIds,
                            exercise.TargetSets,
                            exercise.TargetRepetitions,
                            exercise.TargetRestSeconds,
                            exercise.Notes,
                            exercise.ImageUrl))
                    .ToArray()))
            .ToArray());
    }
}
