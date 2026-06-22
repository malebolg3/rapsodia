// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Domain.Models;
using Rapsodia.Silver.Infrastructure.Data;

namespace Rapsodia.Silver.Infrastructure.Repositories;

public class MemoryGraphRepository : IMemoryGraphRepository
{
    private readonly AppDbContext _db;

    public MemoryGraphRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task SaveMemoryEdge(Guid sourceId, string agentName)
    {
        var node = new GraphEdge
        {
            SourceId = sourceId,
            TargetId = Guid.NewGuid(),
            RelationType = "memory",
            OriginType = "ai-agent",
            TargetType = agentName
        };
        _db.GraphEdges.Add(node);
        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> SearchMemoryEdges(string agentName)
    {
        return await _db.GraphEdges
            .Where(g => g.RelationType == "memory" && g.TargetType == agentName)
            .Select(g => g.SourceId.ToString())
            .ToListAsync();
    }
}