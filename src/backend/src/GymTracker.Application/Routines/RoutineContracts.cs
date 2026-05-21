using GymTracker.Application.Catalog;

namespace GymTracker.Application.Routines;

public sealed record CreateRoutineRequest(
    string Name,
    string? Description,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<CreateRoutineDayRequest> Days);

public sealed record CreateRoutineDayRequest(
    string DayLabel,
    IReadOnlyCollection<CreatePlannedExerciseRequest> Exercises);

public sealed record CreatePlannedExerciseRequest(
    string ExternalExerciseId,
    string ExerciseName,
    IReadOnlyCollection<string> MuscleGroupIds,
    int TargetSets,
    int TargetRepetitions,
    int? TargetRestSeconds,
    string? Notes,
    string? ImageUrl);

public sealed record RoutineSummaryResponse(string Id, string Name, IReadOnlyCollection<string> Tags, int DayCount);

public sealed record RoutineResponse(
    string Id,
    string Name,
    string? Description,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<RoutineDayResponse> Days);

public sealed record RoutineDayResponse(string DayLabel, IReadOnlyCollection<PlannedExerciseResponse> Exercises);

public sealed record PlannedExerciseResponse(
    string ExternalExerciseId,
    string ExerciseName,
    IReadOnlyCollection<string> MuscleGroupIds,
    int TargetSets,
    int TargetRepetitions,
    int? TargetRestSeconds,
    string? Notes,
    string? ImageUrl);

public sealed record SuggestedExercisesResponse(IReadOnlyCollection<ExerciseDto> Exercises);
