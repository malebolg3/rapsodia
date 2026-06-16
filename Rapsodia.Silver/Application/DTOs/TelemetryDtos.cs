using System.ComponentModel.DataAnnotations;

namespace Rapsodia.Silver.Application.DTOs;

public record TelemetryRequest(
    [Required] Guid AgentId,
    [Required, StringLength(20, MinimumLength = 1), RegularExpression(@"^[a-zA-Z0-9_\-]+$")] string SessionId,
    [MaxLength(255), RegularExpression(@"^[a-zA-Z0-9_\-\.\/\\:\s]+$")] string TargetFilePath,
    [Required, Range(0.0, 8.0)] double EntropyValue,
    [Required] DateTime AnalysisTimestamp,
    [Range(0, long.MaxValue)] long ProcessingTimeMs,
    [Required, StringLength(20), RegularExpression(@"^(Low|Medium|High|Critical)$")] string RiskLevel
);

public record TelemetryResponse(
    int Id,
    Guid AgentId,
    string SessionId,
    string TargetFilePath,
    double EntropyValue,
    DateTime AnalysisTimestamp,
    long ProcessingTimeMs,
    string RiskLevel,
    DateTime CreatedAt
);

public record TelemetryStats(
    int TotalRecords,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    double AverageEntropy,
    double MaxEntropy,
    double MinEntropy,
    long AverageProcessingMs,
    int UniqueAgents,
    int UniqueSessions,
    DateTime? LastEventAt,
    List<HourlyBucket> HourlyDistribution,
    List<RiskBucket> RiskDistribution
);

public record HourlyBucket(string Hour, int Count, double AvgEntropy);
public record RiskBucket(string RiskLevel, int Count, double Percentage);