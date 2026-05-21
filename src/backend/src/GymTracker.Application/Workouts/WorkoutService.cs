using GymTracker.Application.Progress;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Services;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Workouts;

public sealed class WorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IProgressSnapshotRepository _progressSnapshotRepository;
    private readonly ProgressCalculationService _progressCalculationService;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IProgressSnapshotRepository progressSnapshotRepository,
        ProgressCalculationService progressCalculationService)
    {
        _workoutRepository = workoutRepository;
        _progressSnapshotRepository = progressSnapshotRepository;
        _progressCalculationService = progressCalculationService;
    }

    public async Task<WorkoutResponse> CreateAsync(string userId, CreateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = WorkoutValidators.Validate(request);
        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(string.Join(' ', validationErrors));
        }

        var workout = new Workout
        {
            UserId = userId,
            PerformedAt = request.PerformedAt,
            Status = string.Equals(request.Status, "incomplete", StringComparison.OrdinalIgnoreCase)
                ? WorkoutStatus.Incomplete
                : WorkoutStatus.Completed,
            RoutineId = request.RoutineId,
            Notes = request.Notes,
            ExerciseEntries = request.ExerciseEntries.Select(MapExerciseEntry).ToArray(),
        };

        await _workoutRepository.AddAsync(workout, cancellationToken);

        foreach (var exerciseId in workout.ExerciseEntries.Select(entry => entry.ExternalExerciseId).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var workouts = await _workoutRepository.ListByUserAndExerciseAsync(userId, exerciseId, cancellationToken);
            var result = _progressCalculationService.Calculate(exerciseId, workouts, DateTimeOffset.UtcNow);
            var snapshot = new ProgressSnapshot
            {
                UserId = userId,
                ExerciseId = exerciseId,
                WorkoutId = workout.Id,
                Result = result,
            };

            await _progressSnapshotRepository.SaveAsync(snapshot, cancellationToken);
        }

        return WorkoutResponseMapper.ToResponse(workout);
    }

    public async Task<WorkoutResponse?> GetByIdAsync(string userId, string workoutId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(userId, workoutId, cancellationToken);
        if (workout is null)
        {
            return null;
        }

        return WorkoutResponseMapper.ToResponse(workout);
    }

    public async Task<WorkoutResponse> UpdateAsync(string userId, string workoutId, UpdateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = WorkoutValidators.Validate(request);
        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(string.Join(' ', validationErrors));
        }

        var existingWorkout = await _workoutRepository.GetByIdAsync(userId, workoutId, cancellationToken);
        if (existingWorkout is null)
        {
            throw new ArgumentException("Entrenamiento no encontrado.");
        }

        var workout = new Workout
        {
            Id = existingWorkout.Id,
            UserId = userId,
            PerformedAt = request.PerformedAt,
            Status = string.Equals(request.Status, "incomplete", StringComparison.OrdinalIgnoreCase)
                ? WorkoutStatus.Incomplete
                : WorkoutStatus.Completed,
            RoutineId = request.RoutineId,
            Notes = request.Notes,
            ExerciseEntries = request.ExerciseEntries.Select(MapExerciseEntry).ToArray(),
        };

        await _workoutRepository.UpdateAsync(userId, workout, cancellationToken);

        // Recalculate progress for affected exercises
        var oldExerciseIds = existingWorkout.ExerciseEntries.Select(entry => entry.ExternalExerciseId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var newExerciseIds = workout.ExerciseEntries.Select(entry => entry.ExternalExerciseId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var allAffectedExerciseIds = oldExerciseIds.Concat(newExerciseIds).Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var exerciseId in allAffectedExerciseIds)
        {
            var workouts = await _workoutRepository.ListByUserAndExerciseAsync(userId, exerciseId, cancellationToken);
            var result = _progressCalculationService.Calculate(exerciseId, workouts, DateTimeOffset.UtcNow);
            var snapshot = new ProgressSnapshot
            {
                UserId = userId,
                ExerciseId = exerciseId,
                WorkoutId = workout.Id,
                Result = result,
            };

            await _progressSnapshotRepository.SaveAsync(snapshot, cancellationToken);
        }

        return WorkoutResponseMapper.ToResponse(workout);
    }

    public async Task DeleteAsync(string userId, string workoutId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(userId, workoutId, cancellationToken);
        if (workout is null)
        {
            throw new ArgumentException("Entrenamiento no encontrado.");
        }

        await _workoutRepository.DeleteAsync(userId, workoutId, cancellationToken);

        // Recalculate progress for affected exercises
        foreach (var exerciseId in workout.ExerciseEntries.Select(entry => entry.ExternalExerciseId).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var workouts = await _workoutRepository.ListByUserAndExerciseAsync(userId, exerciseId, cancellationToken);
            if (workouts.Count > 0)
            {
                var result = _progressCalculationService.Calculate(exerciseId, workouts, DateTimeOffset.UtcNow);
                var snapshot = new ProgressSnapshot
                {
                    UserId = userId,
                    ExerciseId = exerciseId,
                    WorkoutId = workoutId,
                    Result = result,
                };

                await _progressSnapshotRepository.SaveAsync(snapshot, cancellationToken);
            }
        }
    }

    private static ExerciseEntry MapExerciseEntry(CreateExerciseEntryRequest entry)
    {
        var capturedAt = DateTimeOffset.UtcNow;

        return new ExerciseEntry
        {
            ExternalExerciseId = entry.ExternalExerciseId,
            ExerciseNameSnapshot = entry.ExerciseName,
            MuscleGroupIds = entry.MuscleGroupIds,
            Notes = entry.Notes,
            ImageUrl = entry.ImageUrl,
            ExerciseSnapshot = new ExerciseSnapshot(
                ExerciseId: entry.ExternalExerciseId,
                Name: entry.ExerciseName,
                CoverStoragePath: entry.ImageUrl,
                FormTypeId: entry.FormTypeId ?? string.Empty,
                FormTypeCode: entry.FormTypeCode ?? string.Empty,
                CapturedAt: capturedAt),
            Sets = entry.Sets.Select(setEntry => new WorkoutSet
            {
                Repetitions = setEntry.Repetitions,
                Weight = setEntry.Weight,
                RestSeconds = setEntry.RestSeconds,
                Completed = setEntry.Completed,
            }).ToArray(),
        };
    }
}

