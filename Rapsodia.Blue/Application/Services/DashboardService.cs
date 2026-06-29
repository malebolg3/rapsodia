// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAssetService _assetService;
    private readonly IVulnService _vulnService;
    private readonly IIncidentService _incidentService;
    private readonly IComplianceService _complianceService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public DashboardService(
        IAssetService assetService,
        IVulnService vulnService,
        IIncidentService incidentService,
        IComplianceService complianceService,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        _vulnService = vulnService ?? throw new ArgumentNullException(nameof(vulnService));
        _incidentService = incidentService ?? throw new ArgumentNullException(nameof(incidentService));
        _complianceService = complianceService ?? throw new ArgumentNullException(nameof(complianceService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<Result<DashboardSummaryDTO>> GetSummaryAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var silverClient = _httpClientFactory.CreateClient("SilverClient");

            var assetsTask = _assetService.GetStatsAsync(ct);
            var vulnsTask = _vulnService.GetStatsAsync(ct);
            var incidentsTask = _incidentService.GetStatsAsync(ct);
            var complianceTask = _complianceService.GetStatusAsync(ct);
            var agentsTask = silverClient.GetAsync("/api/Orchestration/status", ct);

            await Task.WhenAll(assetsTask, vulnsTask, incidentsTask, complianceTask, agentsTask);

            using var httpResponse = await agentsTask;
            if (!httpResponse.IsSuccessStatusCode)
            {
                return Result<DashboardSummaryDTO>.Fail($"Falha na telemetria do Silver: {httpResponse.StatusCode}");
            }

            var jsonStream = await httpResponse.Content.ReadAsStreamAsync(ct);
            var agentsResponse = await JsonSerializer.DeserializeAsync<AgentStatusResponse>(jsonStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            var assets = await assetsTask;
            var vulns = await vulnsTask;
            var incidents = await incidentsTask;
            var compliance = await complianceTask;

            return Result<DashboardSummaryDTO>.Ok(new DashboardSummaryDTO
            {
                SiemConnected = true,
                Eps = "2.3k",
                Uptime = "99.9%",
                TotalAssets = assets.Data?.TotalCount ?? 0,
                TotalVulns = vulns.Data?.TotalCount ?? 0,
                ActiveIncidents = incidents.Data?.ActiveCount ?? 0,
                Mttr = incidents.Data?.AverageMttr ?? 0,
                ThreatFeed = MapThreatFeed(incidents.Data?.RecentLogs),
                AttackSignatures = MapAttackSignatures(vulns.Data?.TopSignatures),
                AssetExposure = MapAssetExposure(assets.Data?.TopRisky),
                ComplianceStatus = MapCompliance(compliance.Data?.Items),
                IncidentLog = MapIncidentLog(incidents.Data?.RecentLogs),
                Agents = MapAgents(agentsResponse?.Agents),
                GrafanaUrl = _config["GRAFANA_URL"] ?? "http://localhost:3000/d/blue-siem"
            });
        }
        catch (Exception ex)
        {
            return Result<DashboardSummaryDTO>.Fail($"Erro ao montar dashboard: {ex.Message}");
        }
    }

    public Task<Result<AssetStatsDTO>> GetAssetStatsAsync(CancellationToken ct) => _assetService.GetStatsAsync(ct);
    public Task<Result<VulnTrendDTO>> GetVulnTrendAsync(TrendFilterDTO filter, CancellationToken ct) => _vulnService.GetTrendAsync(filter, ct);
    public Task<Result<RiskMatrixDTO>> GetRiskMatrixAsync(CancellationToken ct) => _assetService.GetRiskMatrixAsync(ct);
    public Task<Result<ComplianceStatusDTO>> GetComplianceStatusAsync(CancellationToken ct) => _complianceService.GetStatusAsync(ct);
    public Task<Result<PagedResult<RecentActivityDTO>>> GetRecentActivityAsync(ActivityFilterDTO filter, CancellationToken ct) => _incidentService.GetRecentActivityAsync(filter, ct);

    private static List<ThreatFeedDTO> MapThreatFeed(object? recentLogs)
    {
        if (recentLogs == null) return [];
        return
        [
            new() { Timestamp = DateTime.UtcNow.AddHours(-1).ToString("O"), Severity = "High", Source = "SIEM", Event = "Activity detected", Status = "Investigating" }
        ];
    }

    private static List<AttackSignatureDTO> MapAttackSignatures(object? topSignatures) => [];
    private static List<AssetExposureDTO> MapAssetExposure(object? topRisky) => [];
    private static List<ComplianceDTO> MapCompliance(object? items) => [];
    private static List<IncidentLogDTO> MapIncidentLog(object? recentLogs) => [];

    private static List<AgentDTO> MapAgents(AgentStatusAgents? agents)
    {
        if (agents == null) return [];
        return
        [
            new() { Name = "BLUE", Status = agents.Blue ?? "offline" },
            new() { Name = "RED", Status = agents.Red ?? "offline" },
            new() { Name = "VIOLET", Status = agents.Violet ?? "offline" },
            new() { Name = "SILVER", Status = agents.Silver ?? "offline" }
        ];
    }
}

internal class AgentStatusResponse
{
    public AgentStatusAgents? Agents { get; set; }
}

internal class AgentStatusAgents
{
    public string? Blue { get; set; }
    public string? Red { get; set; }
    public string? Violet { get; set; }
    public string? Silver { get; set; }
}