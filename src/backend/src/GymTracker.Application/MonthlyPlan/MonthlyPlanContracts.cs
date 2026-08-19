namespace GymTracker.Application.MonthlyPlan;

public sealed record MonthlyPlanExerciseRequest(Guid ExerciseId, int OrderIndex);

public sealed record MonthlyPlanDayRequest(int WeekNumber, int DayNumber, IReadOnlyCollection<MonthlyPlanExerciseRequest> Exercises);

public sealed record UpsertMonthlyPlanRequest(int ActiveDays, IReadOnlyCollection<MonthlyPlanDayRequest> Days);

public sealed record LinkExerciseToDayRequest(int WeekId, int DayId, Guid ExerciseId);

public sealed record UpdatePlannedExerciseRequest(Guid ExerciseId);

public sealed record MonthlyPlanExerciseResponse(
    Guid Id,
    Guid ExerciseId,
    int OrderIndex,
    string? ExerciseName = null,
    string? ExerciseType = null,
    IReadOnlyCollection<string>? PrimaryMuscles = null);

public sealed record MonthlyPlanDayResponse(int WeekNumber, int DayNumber, string Status, bool IsTruncated, IReadOnlyCollection<MonthlyPlanExerciseResponse> Exercises);

public sealed record MonthlyPlanWeekResponse(int WeekNumber, IReadOnlyCollection<MonthlyPlanDayResponse> Days);

public sealed record MonthlyPlanResponse(
	Guid Id,
	int ActiveDays,
	IReadOnlyCollection<MonthlyPlanWeekResponse> Weeks);

public sealed record TruncateDaysRequest(int ActiveDays, bool Confirmed);
