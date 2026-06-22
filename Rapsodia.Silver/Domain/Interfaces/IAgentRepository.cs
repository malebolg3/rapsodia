// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Domain.Interfaces;

public interface IAgentRepository
{
    Task<int?> GetAgentIdByName(string agentName);
}