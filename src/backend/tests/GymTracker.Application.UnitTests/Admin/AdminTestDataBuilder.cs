using GymTracker.Domain.Entities;

namespace GymTracker.Application.UnitTests.Admin;

public static class AdminTestDataBuilder
{
    public static Muscle CreateActiveMuscle(
        string code = "BICEPS",
        string name = "Biceps",
        string? description = "Brazo",
        params string[] muscleGroupIds)
    {
        var groups = muscleGroupIds.Length > 0 ? muscleGroupIds : new[] { "general" };
        return Muscle.Create(name, code, description, groups);
    }

    public static Muscle CreateDeletedMuscle(
        string code = "DELETED_MUSCLE",
        string name = "Deleted",
        string? description = null,
        params string[] muscleGroupIds)
    {
        var entity = CreateActiveMuscle(code, name, description, muscleGroupIds);
        entity.SoftDelete(DateTimeOffset.UtcNow);
        return entity;
    }
}
