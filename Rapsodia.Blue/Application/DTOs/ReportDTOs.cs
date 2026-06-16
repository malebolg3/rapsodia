namespace Rapsodia.Blue.Application.DTOs;

public class CreateReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public List<int>? AssetIds { get; set; }
    public List<int>? VulnIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Format { get; set; }
}

public class ReportFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ReportType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ReportResultDTO
{
    public int Id { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Format { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class ReportFileDTO
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class DashboardDTO
{
    public int TotalAssets { get; set; }
    public int TotalVulnerabilities { get; set; }
    public int ActiveIncidents { get; set; }
    public int CompletedScans { get; set; }
    public Dictionary<string, int> VulnsBySeverity { get; set; } = new();
    public List<RecentActivityDTO> RecentActivities { get; set; } = new();
}

public class RecentActivityDTO
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}