// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Domain.Interfaces;

public interface IMemoryGraphRepository
{
    Task SaveMemoryEdge(Guid sourceId, string agentName);
    Task<List<string>> SearchMemoryEdges(string agentName);
}