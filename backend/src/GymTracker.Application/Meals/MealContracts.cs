namespace GymTracker.Application.Meals;

public sealed record MealItemInput(string ExternalFoodId, double Quantity, string Unit, double? Calories);

public sealed record CreateMealLogRequest(DateOnly LoggedDate, string SlotType, IReadOnlyCollection<MealItemInput> Items);

public sealed record MealLogResponse(string Id, DateOnly LoggedDate, string SlotType, IReadOnlyCollection<MealItemInput> Items, double? TotalCalories);
