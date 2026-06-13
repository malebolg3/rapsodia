namespace Rapsodia.Blue.Application.DTOs;

public class TrendFilterDTO
{
    public string? MetricType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Interval { get; set; }
}

public class ActivityFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ActivityType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class AssetStatsDTO
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
}

public class VulnTrendDTO
{
    public List<TrendDataPointDTO> DataPoints { get; set; } = new();
    public string MetricType { get; set; } = string.Empty;
}

public class TrendDataPointDTO
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public string? Label { get; set; }
}

public class RiskMatrixDTO
{
    public Dictionary<string, int> ImpactVs { get; set; } = new();
    public List<RiskItemDTO> TopRisks { get; set; } = new();
}

public class RiskItemDTO
{
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Level { get; set; } = string.Empty;
}

public class ComplianceStatusDTO
{
    public double OverallScore { get; set; }
    public List<ComplianceItemDTO> Items { get; set; } = new();
}

public class ComplianceItemDTO
{
    public string Framework { get; set; } = string.Empty;
    public string Control { get; set; } = string.Empty;
    public bool Compliant { get; set; }
    public double Score { get; set; }
}