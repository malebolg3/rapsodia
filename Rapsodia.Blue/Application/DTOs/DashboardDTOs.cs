// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

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
    public int TotalCount { get; set; }
    public Dictionary<string, int> ByType { get; set; } = [];
    public List<AssetExposureDTO> TopRisky { get; set; } = [];
}

public class VulnStatsDTO
{
    public int TotalCount { get; set; }
    public List<AttackSignatureDTO> TopSignatures { get; set; } = [];
}

public class VulnTrendDTO
{
    public List<TrendDataPointDTO> DataPoints { get; set; } = [];
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
    public Dictionary<string, int> ImpactVs { get; set; } = [];
    public List<RiskItemDTO> TopRisks { get; set; } = [];
}

public class RiskItemDTO
{
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Level { get; set; } = string.Empty;
}

public class ComplianceStatusDTO
{
    public double OverallScore { get; set; }
    public List<ComplianceItemDTO> Items { get; set; } = [];
}

public class ComplianceItemDTO
{
    public string Framework { get; set; } = string.Empty;
    public string Control { get; set; } = string.Empty;
    public bool Compliant { get; set; }
    public double Score { get; set; }
}