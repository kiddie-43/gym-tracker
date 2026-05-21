namespace GymTracker.Application.Catalog;

public sealed record MuscleGroupDto(string Id, string Name);

public sealed record ExerciseDto(string Id, string Name, IReadOnlyCollection<string> MuscleGroupIds, string? ImageUrl);

public sealed record FoodDto(string Id, string Name, double? Calories);

public sealed record CatalogAvailabilityDto(bool IsStale, DateTimeOffset LastUpdatedAt);
