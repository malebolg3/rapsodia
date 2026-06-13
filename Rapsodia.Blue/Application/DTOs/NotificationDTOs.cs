namespace Rapsodia.Blue.Application.DTOs;

public class NotificationFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsRead { get; set; }
    public string? Type { get; set; }
}

public class NotificationResultDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationPreferencesRequest
{
    public bool EmailEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public List<string>? SubscribedTypes { get; set; }
}

public class UnreadCountDTO
{
    public int Total { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
}