// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Collections.Generic;

namespace Rapsodia.Blue.Application.DTOs;

public class DashboardSummaryDTO
{
    public bool SiemConnected { get; set; }
    public string Eps { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public int TotalAssets { get; set; }
    public int TotalVulns { get; set; }
    public int ActiveIncidents { get; set; }
    public int Mttr { get; set; }
    public List<ThreatFeedDTO> ThreatFeed { get; set; } = [];
    public List<AttackSignatureDTO> AttackSignatures { get; set; } = [];
    public List<AssetExposureDTO> AssetExposure { get; set; } = [];
    public List<ComplianceDTO> ComplianceStatus { get; set; } = [];
    public List<IncidentLogDTO> IncidentLog { get; set; } = [];
    public List<AgentDTO> Agents { get; set; } = [];
    public string GrafanaUrl { get; set; } = string.Empty;
}