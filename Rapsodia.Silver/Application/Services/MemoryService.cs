// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Infrastructure.Data;
using Rapsodia.Silver.Infrastructure.Services;
using Rapsodia.Silver.Domain.Models;

namespace Rapsodia.Silver.Application.Services;

public class MemoryService : IMemoryService
{
    private readonly IObsidianService _obsidian;
    private readonly AppDbContext _db;
    private readonly EmbeddingService _embedding;
    private readonly ILogger<MemoryService> _logger;

    public MemoryService(IObsidianService obsidian, AppDbContext db, EmbeddingService embedding, ILogger<MemoryService> logger)
    {
        _obsidian = obsidian ?? throw new ArgumentNullException(nameof(obsidian));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _embedding = embedding ?? throw new ArgumentNullException(nameof(embedding));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveMemory(string agent, string prompt, string response)
    {
        var sanitizedAgent = SanitizeFrontMatter(agent);
        var markdown = await GenerateMarkdown(sanitizedAgent, prompt, response);
        float[] embedding = await GetEmbeddingSafeAsync($"{prompt} {response}");

        try
        {
            await _obsidian.AppendNoteAsync("ai-analysis", markdown);
            _logger.LogInformation("Memória persistida no Obsidian para agente: {Agent}", sanitizedAgent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Obsidian offline. Redirecionando memória para Graph.");
            
            var node = new GraphEdge
            {
                SourceId = Guid.NewGuid(),
                TargetId = Guid.NewGuid(),
                RelationType = "memory",
                OriginType = "ai-agent",
                TargetType = sanitizedAgent,
                Weight = 1.0f,
                Embedding = embedding.Length > 0 ? string.Join(",", embedding.Select(f => f.ToString("F6"))) : null
            };

            _db.GraphEdges.Add(node);
            await _db.SaveChangesAsync();
        }
    }

    public Task<string> GenerateMarkdown(string agent, string prompt, string response)
    {
        var markdown = $"""
            ---
            agent: {SanitizeFrontMatter(agent)}
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
        var sanitizedAgent = SanitizeFrontMatter(agent);

        try
        {
            var obsidianResults = await _obsidian.SearchNotesAsync("ai-analysis", query);
            if (obsidianResults?.Any() == true)
                results.AddRange(obsidianResults.Select(r => $"obsidian:{r}"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha na varredura do Obsidian.");
        }

        var graphResults = await _db.GraphEdges
            .Where(g => g.RelationType == "memory" && g.TargetType == sanitizedAgent)
            .AsNoTracking()
            .Take(100)
            .ToListAsync();

        if (!graphResults.Any()) return results;

        float[] queryEmbedding = await GetEmbeddingSafeAsync(query);

        if (queryEmbedding.Length > 0)
        {
            var scored = graphResults
                .Where(g => !string.IsNullOrEmpty(g.Embedding))
                .Select(g => new
                {
                    Id = g.SourceId.ToString(),
                    Score = EmbeddingService.CosineSimilarity(queryEmbedding, g.Embedding!.Split(',').Select(float.Parse).ToArray())
                })
                .Where(x => x.Score >= 0.70)
                .OrderByDescending(x => x.Score)
                .Take(5);

            results.AddRange(scored.Select(x => $"semantic:{x.Id} (score: {x.Score:F2})"));
        }

        results.AddRange(graphResults
            .Where(g => string.IsNullOrEmpty(g.Embedding))
            .Select(g => $"graph:{g.SourceId}"));

        return results;
    }

    private async Task<float[]> GetEmbeddingSafeAsync(string text)
    {
        try
        {
            return await _embedding.GetEmbeddingAsync(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha na geração de vetores semânticos.");
            return Array.Empty<float>();
        }
    }

    private static string SanitizeFrontMatter(string value)
    {
        return value.Replace("\n", " ").Replace("\r", " ").Replace("---", "").Trim();
    }
}