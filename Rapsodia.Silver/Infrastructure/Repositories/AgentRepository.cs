// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Infrastructure.Data;

namespace Rapsodia.Silver.Infrastructure.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly AppDbContext _db;

    public AgentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int?> GetAgentIdByName(string agentName)
    {
        return await _db.AgentMetadata
            .Where(a => a.AgentId == agentName)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
    }
}