namespace Rapsodia.Silver.Domain.Models;

public abstract class BaseEntity
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
}