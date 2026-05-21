using GymTracker.Application.Catalog;

namespace GymTracker.Infrastructure.Wger;

public sealed class WgerCatalogMapper
{
    public MuscleGroupDto MapMuscleGroup(int id, string name)
    {
        return new MuscleGroupDto(id.ToString(), name);
    }

    public ExerciseDto MapExercise(int id, string name, IReadOnlyCollection<int> muscles, string? imageUrl)
    {
        return new ExerciseDto(id.ToString(), name, muscles.Select(muscle => muscle.ToString()).ToArray(), imageUrl);
    }

    public FoodDto MapFood(int id, string name, double? calories)
    {
        return new FoodDto(id.ToString(), name, calories);
    }
}
