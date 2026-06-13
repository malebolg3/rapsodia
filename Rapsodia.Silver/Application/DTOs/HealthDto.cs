namespace Rapsodia.Silver.Application.DTOs;

public record HealthDto(string Status, DateTime Timestamp, bool IsDatabaseHealthy);