// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Domain.Models;
using Rapsodia.Silver.Infrastructure.Data;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class SilverAgent : Grain, ISilverAgent
{
    private string _context = "Você é um analista de cibersegurança especializado em identificar vulnerabilidades e sugerir remediações.";
    private readonly HttpClient _aiClient;
    private readonly IObsidianService _obsidian;
    private readonly IMemoryService _memory;
    private readonly IConfiguration _cfg;
    private readonly ILogger<SilverAgent> _logger;
    private readonly EventPublisher _publisher;
    private readonly TelemetryService _telemetry;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<AgentInfo> _managedAgents = new();

    public SilverAgent(IHttpClientFactory httpFactory, IObsidianService obsidian, IMemoryService memory,
        IConfiguration cfg, ILogger<SilverAgent> logger, EventPublisher publisher, 
        TelemetryService telemetry, IServiceProvider serviceProvider)
    {
        _aiClient = httpFactory.CreateClient("AIAgent");
        _obsidian = obsidian;
        _memory = memory;
        _cfg = cfg;
        _logger = logger;
        _publisher = publisher;
        _telemetry = telemetry;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> AnalyzeAsync(string input)
    {
        var model = _cfg["AI_MODEL"] ?? "mistral";
        var aiUrl = _cfg["AI_URL"] ?? "http://localhost:11434";
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Silver: Analisando com modelo {Model}", model);

        try
        {
            var payload = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = _context },
                    new { role = "user", content = input }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _aiClient.PostAsync($"{aiUrl}/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            
            sw.Stop();
            _telemetry.RecordAIAnalysis(model);
            _telemetry.RecordAIAnalysisDuration(sw.Elapsed.TotalSeconds);
            
            await _memory.SaveMemory(model, input, result);
            
            await _publisher.PublishAsync("Default", "ai-events", Guid.NewGuid(), new
            {
                type = "ai.analysis.completed",
                model,
                duration = sw.Elapsed.TotalSeconds,
                timestamp = DateTime.UtcNow
            });
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Silver: Falha na análise IA");
            return $"{{\"error\": \"{ex.Message}\", \"mock\": true, \"findings\": [\"Verificar manualmente\"]}}";
        }
    }

    public Task<string> GetStatusAsync()
    {
        return Task.FromResult($"Silver Agent Ativo | Modelo: {_cfg["AI_MODEL"] ?? "mistral"} | Agentes gerenciados: {_managedAgents.Count(a => a.IsActive)} | Obsidian: conectado");
    }

    public Task SetContextAsync(string context)
    {
        _context = context;
        _logger.LogInformation("Silver: Contexto atualizado");
        return Task.CompletedTask;
    }

    public async Task<string> CreateAgent(string agentType, string expertise, string ownerId)
    {
        var agentId = Guid.NewGuid().ToString("N")[..12];
        
        var agent = new AgentInfo
        {
            AgentId = agentId,
            AgentType = agentType,
            Expertise = expertise,
            OwnerId = ownerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _managedAgents.Add(agent);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        db.Set<AgentMetadata>().Add(new AgentMetadata(agentId, agentType, expertise, ownerId));
        
        await db.SaveChangesAsync();
        
        _logger.LogInformation("Silver: Agente {AgentId} criado ({Type}: {Expertise})", agentId, agentType, expertise);
        
        return agentId;
    }

    public Task<List<AgentInfo>> ListAgents()
    {
        return Task.FromResult(_managedAgents.ToList());
    }

    public async Task<bool> DeactivateAgent(string agentId)
    {
        var agent = _managedAgents.FirstOrDefault(a => a.AgentId == agentId);
        if (agent == null) return false;
        
        agent.IsActive = false;
        agent.DeactivatedAt = DateTime.UtcNow;
        
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var metadata = await db.Set<AgentMetadata>().FirstOrDefaultAsync(a => a.AgentId == agentId);
        if (metadata != null)
        {
            metadata.MarkAsDeleted();
            await db.SaveChangesAsync();
        }
        
        _logger.LogInformation("Silver: Agente {AgentId} desativado", agentId);
        return true;
    }

    public async Task<bool> ActivateAgent(string agentId)
    {
        var agent = _managedAgents.FirstOrDefault(a => a.AgentId == agentId);
        if (agent == null) return false;
        
        agent.IsActive = true;
        agent.DeactivatedAt = null;
        
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var metadata = await db.Set<AgentMetadata>().FirstOrDefaultAsync(a => a.AgentId == agentId);
        if (metadata != null)
        {
            metadata.MarkAsRestored();
            await db.SaveChangesAsync();
        }
        
        _logger.LogInformation("Silver: Agente {AgentId} reativado", agentId);
        return true;
    }

    public async Task<List<string>> SearchMemory(string query)
    {
        var model = _cfg["AI_MODEL"] ?? "mistral";
        return await _memory.SearchMemory(query, model);
    }
}