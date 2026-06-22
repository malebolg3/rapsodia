// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Infrastructure.Data;
using Rapsodia.Silver.Domain.Models;

namespace Rapsodia.Silver.Application.Services;

public class MemoryService : IMemoryService
{
    private readonly IObsidianService _obsidian;
    private readonly AppDbContext _db;
    private readonly ILogger<MemoryService> _logger;

    public MemoryService(IObsidianService obsidian, AppDbContext db, ILogger<MemoryService> logger)
    {
        _obsidian = obsidian;
        _db = db;
        _logger = logger;
    }

    public async Task SaveMemory(string agent, string prompt, string response)
    {
        var markdown = await GenerateMarkdown(agent, prompt, response);

        try
        {
            await _obsidian.AppendNoteAsync("ai-analysis", markdown);
            _logger.LogInformation("Memória salva no Obsidian (agente: {Agent})", agent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Obsidian indisponível. Salvando no Graph (fallback)");

            var agentEntity = await _db.AgentMetadata
                .FirstOrDefaultAsync(a => a.AgentId == agent);
            var targetId = agentEntity?.Id ?? 0;

            var node = new GraphEdge
            {
                SourceId = Guid.NewGuid(),
                TargetId = Guid.NewGuid(),
                RelationType = "memory",
                OriginType = "ai-agent",
                TargetType = agent
            };
            _db.GraphEdges.Add(node);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Memória salva no Graph (agente: {Agent})", agent);
        }
    }

    public Task<string> GenerateMarkdown(string agent, string prompt, string response)
    {
        var markdown = $"""
            ---
            agent: {agent}
            timestamp: {DateTime.UtcNow:O}
            ---

            ## 🧠 Memória - {agent}

            **Prompt:** {prompt}

            **Resposta:** {response}

            ---
            """;

        return Task.FromResult(markdown);
    }

    public async Task<List<string>> SearchMemory(string query, string agent)
    {
        var results = new List<string>();

        try
        {
            var obsidianResults = await _obsidian.SearchNotesAsync("ai-analysis", query);
            if (obsidianResults.Any())
                results.AddRange(obsidianResults.Select(r => $"obsidian:{r}"));
        }
        catch
        {
            _logger.LogWarning("Busca no Obsidian falhou, tentando Graph");
        }

        var agentId = await _db.AgentMetadata
            .Where(a => a.AgentId == agent)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();

        if (agentId != 0)
        {
            var graphResults = await _db.GraphEdges
                .Where(g => g.RelationType == "memory" && g.TargetType == agent)
                .Select(g => g.SourceId.ToString())
                .ToListAsync();

            results.AddRange(graphResults.Select(r => $"graph:{r}"));
        }

        return results;
    }
}