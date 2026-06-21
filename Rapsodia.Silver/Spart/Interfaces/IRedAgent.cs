// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Spart.Interfaces;

public interface IRedAgent : IGrainWithIntegerKey
{
    Task<string> GetStatusAsync();
    Task<string> StartScanAsync(string target, string scanType);
    Task<string> StartExploitAsync(string target, string exploitName, Dictionary<string, object>? payload);
    Task<string> GetScanResultAsync(string scanId);
}