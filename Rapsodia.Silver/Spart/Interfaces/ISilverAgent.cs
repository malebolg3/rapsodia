// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Spart.Interfaces;

public interface ISilverAgent : IGrainWithIntegerKey
{
    Task<string> AnalyzeAsync(string input);
    Task<string> GetStatusAsync();
    Task SetContextAsync(string context);
}