namespace GymTracker.Application.Workouts;

public sealed record CreateWorkoutRequest(
    DateTimeOffset PerformedAt,
    string Status,
    string? RoutineId,
    string? Notes,
    IReadOnlyCollection<CreateExerciseEntryRequest> ExerciseEntries);

public sealed record CreateExerciseEntryRequest(
    string ExternalExerciseId,
    string ExerciseName,
    IReadOnlyCollection<string> MuscleGroupIds,
    IReadOnlyCollection<CreateWorkoutSetRequest> Sets,
    string? Notes,
    string? ImageUrl);

public sealed record CreateWorkoutSetRequest(int Repetitions, decimal? Weight, int? RestSeconds, bool Completed);

public sealed record UpdateWorkoutRequest(
    DateTimeOffset PerformedAt,
    string Status,
    string? RoutineId,
    string? Notes,
    IReadOnlyCollection<CreateExerciseEntryRequest> ExerciseEntries);

public sealed record WorkoutSetResponse(int Repetitions, decimal? Weight, int? RestSeconds, bool Completed);

public sealed record WorkoutExerciseEntryResponse(
    string ExternalExerciseId,
    string ExerciseName,
    string ExerciseNameSnapshot,
    IReadOnlyCollection<string> MuscleGroupIds,
    IReadOnlyCollection<WorkoutSetResponse> Sets,
    string? Notes,
    string? ImageUrl);

public sealed record WorkoutResponse(
    string Id,
    DateTimeOffset PerformedAt,
    string Status,
    string? RoutineId = null,
    string? Notes = null,
    IReadOnlyCollection<WorkoutExerciseEntryResponse>? ExerciseEntries = null);
