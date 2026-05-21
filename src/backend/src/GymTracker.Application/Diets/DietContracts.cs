namespace GymTracker.Application.Diets;

public sealed record CreateDietRequest(string Name, IReadOnlyCollection<DietDayInput> Days);

public sealed record DietDayInput(string DayKey, IReadOnlyCollection<MealSlotInput> MealSlots);

public sealed record MealSlotInput(string SlotType, IReadOnlyCollection<MealItemInput> Items);

public sealed record MealItemInput(string ExternalFoodId, double Quantity, string Unit, double? Calories);

public sealed record DietSummaryResponse(string Id, string Name, int DayCount);

public sealed record DietResponse(string Id, string Name, IReadOnlyCollection<DietDayInput> Days);
