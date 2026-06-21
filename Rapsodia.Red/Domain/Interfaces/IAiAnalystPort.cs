// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Red.Domain.Interfaces;

public interface IAiAnalystPort
{
    Task<string> AnalyzeScanResultsAsync(string toolName, string rawOutput);
}
