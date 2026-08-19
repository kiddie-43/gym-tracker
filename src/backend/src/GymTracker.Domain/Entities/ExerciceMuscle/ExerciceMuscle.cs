namespace GymTracker.Domain.Entities;

using GymTracker.Domain.Common;

public sealed class ExerciceMuscle : AuditableEntity
{
    public Guid ExerciceId { get; private set; }

    public Exercice Exercice { get; private set; } = null!;

    public Guid MuscleId { get; private set; }

    public Muscle Muscle { get; private set; } = null!;

    public ExerciceMuscleType Type { get; private set; }

    private ExerciceMuscle()
    {
    }

    public ExerciceMuscle(
        Exercice exercice,
        Muscle muscle,
        ExerciceMuscleType type)
    {
        Exercice = exercice ?? throw new ArgumentNullException(nameof(exercice));
        Muscle = muscle ?? throw new ArgumentNullException(nameof(muscle));

        ExerciceId = exercice.Id;
        MuscleId = muscle.Id;
        Type = type;
    }
}