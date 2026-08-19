using GymTracker.Domain.Common;

namespace GymTracker.Domain.Entities
{
    public class ExerciceUnit : AuditableEntity
    {
        public Guid ExerciceId { get; set; }
        public Exercice Exercice { get; set; } = null!;

        public Guid UnitId { get; set; }
        public Units Unit { get; set; } = null!;
        private ExerciceUnit()
        {
            // Required by EF Core
        }


        public ExerciceUnit(Exercice exercice, Units unit)
        {
            Exercice = exercice ?? throw new ArgumentNullException(nameof(exercice));
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            ExerciceId = exercice.Id;
            UnitId = unit.Id;
        }

    }
}