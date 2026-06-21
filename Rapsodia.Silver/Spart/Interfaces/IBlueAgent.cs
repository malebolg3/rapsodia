// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Spart.Interfaces;

public interface IBlueAgent : IGrainWithIntegerKey
{
    Task<string> GetStatusAsync();
    Task TrackScanAsync(string scanId, string target);
    Task TrackVulnAsync(string vulnId, string severity);
    Task<string> GetAssetSummaryAsync();
    Task NotifyIncidentAsync(string incidentId, string title, string severity);
}