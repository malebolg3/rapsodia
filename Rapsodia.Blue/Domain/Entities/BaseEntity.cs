namespace Rapsodia.Blue.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public bool IsDeleted => DeletedAt.HasValue;

    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
    public void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;
    public void MarkAsRestored() => DeletedAt = null;
}