namespace Rapsodia.Blue.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; protected set; } = string.Empty;
    public string PasswordHash { get; protected set; } = string.Empty;
    public string Role { get; protected set; } = "Analyst";
}