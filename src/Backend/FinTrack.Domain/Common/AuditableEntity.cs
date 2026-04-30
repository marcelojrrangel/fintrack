namespace FinTrack.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; protected set; }

    protected void MarkAsUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
