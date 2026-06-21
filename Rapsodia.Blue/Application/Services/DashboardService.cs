// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IConfiguration _cfg;
    private readonly bool _mock;

    public DashboardService(IConfiguration cfg)
    {
        _cfg = cfg;
        _mock = cfg["DSH_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DB_HOST"]);
    }

    public Task<Result<DashboardDTO>> GetSummaryAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        return Task.FromResult(Result<DashboardDTO>.Ok(new DashboardDTO
        {
            TotalAssets = 156,
            TotalVulnerabilities = 423,
            ActiveIncidents = 7,
            CompletedScans = 89,
            VulnsBySeverity = new Dictionary<string, int>
            {
                ["Critical"] = 12,
                ["High"] = 45,
                ["Medium"] = 134,
                ["Low"] = 232
            },
            RecentActivities = new List<RecentActivityDTO>
            {
                new() { ActivityType = "scan", Description = "Network scan completed on 192.168.1.0/24", Timestamp = DateTime.UtcNow.AddHours(-1) },
                new() { ActivityType = "vuln", Description = "Critical CVE-2024-4321 detected", Timestamp = DateTime.UtcNow.AddHours(-3) },
                new() { ActivityType = "incident", Description = "Incident #7 resolved by Admin", Timestamp = DateTime.UtcNow.AddHours(-5) },
                new() { ActivityType = "asset", Description = "New asset 'DB-Server-03' added", Timestamp = DateTime.UtcNow.AddHours(-8) },
                new() { ActivityType = "scan", Description = "Vulnerability scan completed with 23 findings", Timestamp = DateTime.UtcNow.AddHours(-12) }
            }
        }));
    }

    public Task<Result<AssetStatsDTO>> GetAssetStatsAsync(CancellationToken ct)
    {
        return Task.FromResult(Result<AssetStatsDTO>.Ok(new AssetStatsDTO
        {
            Total = 156,
            Active = 142,
            Inactive = 14,
            ByType = new Dictionary<string, int>
            {
                ["Server"] = 45,
                ["Workstation"] = 78,
                ["Network"] = 18,
                ["Database"] = 10,
                ["Cloud"] = 5
            }
        }));
    }

    public Task<Result<VulnTrendDTO>> GetVulnTrendAsync(TrendFilterDTO filter, CancellationToken ct)
    {
        var dataPoints = new List<TrendDataPointDTO>();
        var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = filter.EndDate ?? DateTime.UtcNow;
        var current = startDate;

        while (current <= endDate)
        {
            dataPoints.Add(new TrendDataPointDTO
            {
                Date = current,
                Count = Random.Shared.Next(5, 30),
                Label = current.ToString("dd/MM")
            });
            current = current.AddDays(1);
        }

        return Task.FromResult(Result<VulnTrendDTO>.Ok(new VulnTrendDTO
        {
            DataPoints = dataPoints,
            MetricType = filter.MetricType ?? "vulnerabilities"
        }));
    }

    public Task<Result<RiskMatrixDTO>> GetRiskMatrixAsync(CancellationToken ct)
    {
        return Task.FromResult(Result<RiskMatrixDTO>.Ok(new RiskMatrixDTO
        {
            ImpactVs = new Dictionary<string, int>
            {
                ["Critical-High"] = 5,
                ["High-High"] = 12,
                ["High-Medium"] = 8,
                ["Medium-Medium"] = 25,
                ["Low-Low"] = 45
            },
            TopRisks = new List<RiskItemDTO>
            {
                new() { AssetId = 10, AssetName = "File-Server-01", Score = 95, Level = "Critical" },
                new() { AssetId = 15, AssetName = "Web-App-Prod", Score = 88, Level = "High" },
                new() { AssetId = 22, AssetName = "DB-Master", Score = 82, Level = "High" },
                new() { AssetId = 8, AssetName = "VPN-Gateway", Score = 75, Level = "Medium" },
                new() { AssetId = 30, AssetName = "Email-Server", Score = 70, Level = "Medium" }
            }
        }));
    }

    public Task<Result<ComplianceStatusDTO>> GetComplianceStatusAsync(CancellationToken ct)
    {
        return Task.FromResult(Result<ComplianceStatusDTO>.Ok(new ComplianceStatusDTO
        {
            OverallScore = 78.5,
            Items = new List<ComplianceItemDTO>
            {
                new() { Framework = "NIST", Control = "AC-1 Access Control Policy", Compliant = true, Score = 95 },
                new() { Framework = "NIST", Control = "AC-2 Account Management", Compliant = true, Score = 88 },
                new() { Framework = "NIST", Control = "AU-1 Audit Policy", Compliant = false, Score = 45 },
                new() { Framework = "ISO 27001", Control = "A.9.2 User access", Compliant = true, Score = 90 },
                new() { Framework = "ISO 27001", Control = "A.12.6 Vulnerability", Compliant = false, Score = 60 },
                new() { Framework = "PCI DSS", Control = "1.1 Firewall Config", Compliant = true, Score = 92 },
                new() { Framework = "PCI DSS", Control = "6.2 Patches", Compliant = false, Score = 55 }
            }
        }));
    }

    public Task<Result<PagedResult<RecentActivityDTO>>> GetRecentActivityAsync(ActivityFilterDTO filter, CancellationToken ct)
    {
        var activities = new List<RecentActivityDTO>
        {
            new() { ActivityType = "scan", Description = "Full vulnerability scan completed", Timestamp = DateTime.UtcNow.AddMinutes(-30) },
            new() { ActivityType = "vuln", Description = "New CVE-2024-5678 identified", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new() { ActivityType = "incident", Description = "Incident #12 opened: Suspicious activity", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new() { ActivityType = "asset", Description = "Asset 'API-Gateway-02' modified", Timestamp = DateTime.UtcNow.AddHours(-3) },
            new() { ActivityType = "user", Description = "User 'analyst1' logged in", Timestamp = DateTime.UtcNow.AddHours(-4) },
            new() { ActivityType = "scan", Description = "Quick scan completed on 10 assets", Timestamp = DateTime.UtcNow.AddHours(-5) },
            new() { ActivityType = "vuln", Description = "Patch applied to 3 critical vulns", Timestamp = DateTime.UtcNow.AddHours(-6) },
            new() { ActivityType = "report", Description = "Monthly security report generated", Timestamp = DateTime.UtcNow.AddHours(-7) }
        };

        var filtered = activities
            .Where(a => string.IsNullOrEmpty(filter.ActivityType) || a.ActivityType == filter.ActivityType)
            .Where(a => !filter.FromDate.HasValue || a.Timestamp >= filter.FromDate.Value)
            .Where(a => !filter.ToDate.HasValue || a.Timestamp <= filter.ToDate.Value)
            .ToList();

        return Task.FromResult(Result<PagedResult<RecentActivityDTO>>.Ok(new PagedResult<RecentActivityDTO>
        {
            Items = filtered.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            TotalCount = filtered.Count,
            Page = filter.Page,
            PageSize = filter.PageSize
        }));
    }
}