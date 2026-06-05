using GymTracker.Domain.Entities;

namespace GymTracker.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; protected set; }

    public Guid CreatedBy { get; protected set; }

    public DateTimeOffset? UpdatedAt { get; protected set; }

    public Guid? UpdatedBy { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public Guid? DeletedBy { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;
       public void Delete(Guid? userId)
    {
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = userId;
    }

    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }
}