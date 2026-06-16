namespace Rapsodia.Blue.Domain.Entities.Olimpo;

public sealed class OlimpoCredential
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string EncryptedValue { get; private set; } = string.Empty;
    public string Environment { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OlimpoCredential() { }

    public static OlimpoCredential Create(Guid userId, string key, string encryptedValue, string environment, string category)
    {
        return new OlimpoCredential
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Key = key,
            EncryptedValue = encryptedValue,
            Environment = environment,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateValue(string encryptedValue)
    {
        EncryptedValue = encryptedValue;
        UpdatedAt = DateTime.UtcNow;
    }
}