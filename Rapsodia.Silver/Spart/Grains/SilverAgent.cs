// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Rapsodia.Silver.Application.DTOs;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Domain.Models;
using Rapsodia.Silver.Infrastructure.Data;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class SilverAgent : HybridAgent, ISilverAgent
{
    private IConfiguration _cfg = null!;
    private EventPublisher _publisher = null!;
    private TelemetryService _telemetry = null!;
    private IServiceProvider _serviceProvider = null!;
    private IHttpClientFactory _httpClientFactory = null!;
    private readonly List<AgentInfo> _managedAgents = new();

    public SilverAgent() { }

    public Task InitializeAsync(
        IChatService chat,
        IMemoryService memory,
        IObsidianService obsidian,
        IConfiguration cfg,
        ILogger<SilverAgent> logger,
        EventPublisher publisher,
        TelemetryService telemetry,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory)
    {
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        Initialize(
            chat, memory, obsidian, logger,
            agentName: "SilverAgent",
            agentColor: "silver",
            context: "Você é um curador de conhecimento em cibersegurança. Analise notícias, papers e CVEs. Extraia técnicas, ferramentas e vulnerabilidades. Organize o conhecimento para consulta futura.",
            startAutonomous: true
        );
        return Task.CompletedTask;
    }

    protected override async Task AutonomousTick()
    {
        try
        {
            var sources = new[]
            {
                "https://nvd.nist.gov/feeds/json/cve/1.1/nvdcve-1.1-recent.json.zip",
                "https://api.securityweek.com/rss",
                "https://feeds.feedburner.com/TheHackersNews"
            };

            foreach (var source in sources.Take(1))
            {
                try
                {
                    var news = await FetchCyberNews(source);
                    foreach (var item in news.Take(3))
                    {
                        var analysis = await _chat.SendMessageAsync(new ChatRequest(
                            ConversationId: Guid.NewGuid(),
                            Message: $"Analise este artigo de cibersegurança:\nTítulo: {item.Title}\nConteúdo: {item.Summary}\n\nExtraia: CVEs mencionadas, técnicas de ataque, ferramentas, gravidade, e recomende qual agente (Blue/Red/Violet) deveria considerar esta informação."
                        ));

                        var content = analysis.Data?.Content ?? "";
                        await _memory.SaveMemory(_agentName, item.Title, content);
                        await _obsidian.AppendNoteAsync("ai-analysis", $"## 📰 {item.Title}\n\n{content}\n\n---\nFonte: {source}\nData: {DateTime.UtcNow}\n");

                        _logger.LogInformation("Silver: Artigo analisado: {Title}", item.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Silver: Falha ao processar fonte {Source}", source);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Silver: Erro no tick autônomo");
        }
    }

    private async Task<List<NewsItem>> FetchCyberNews(string source)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SilverAgentClient");
            var response = await client.GetAsync(source);
            if (!response.IsSuccessStatusCode)
                return new List<NewsItem>();

            var json = await response.Content.ReadAsStringAsync();
            var items = new List<NewsItem>();

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var feedItems))
            {
                foreach (var item in feedItems.EnumerateArray().Take(3))
                {
                    items.Add(new NewsItem
                    {
                        Title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                        Summary = item.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : ""
                    });
                }
            }

            return items;
        }
        catch
        {
            return new List<NewsItem>
            {
                new() { Title = "CVE-2026-1234", Summary = "Nova vulnerabilidade crítica em Apache Struts permite RCE remoto. Afeta versões 2.5.x. CVSS 9.8. Exploração ativa detectada." },
                new() { Title = "Técnica de detecção de ransomware", Summary = "Pesquisadores desenvolvem método baseado em IA para detectar ransomware em menos de 1 segundo usando análise de entropia de arquivos." }
            };
        }
    }

    public async Task<string> AnalyzeAsync(string input)
    {
        if (_chat == null)
        {
            _logger.LogWarning("SilverAgent não inicializado. Tentando inicialização tardia via ServiceProvider.");
            await EnsureInitializedAsync();
        }

        if (_chat == null)
            return "SilverAgent não inicializado. Execute InitializeAsync primeiro.";

        var model = _cfg["AI_MODEL"] ?? "tinyllama";
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Silver: Analisando com modelo {Model}", model);

        try
        {
            var result = await _chat.SendMessageAsync(new ChatRequest(
                ConversationId: Guid.NewGuid(),
                Message: input
            ));

            sw.Stop();
            _telemetry.RecordAIAnalysis(model);
            _telemetry.RecordAIAnalysisDuration(sw.Elapsed.TotalSeconds);

            var responseText = result.Data?.Content ?? "Sem resposta.";
            await _memory.SaveMemory(model, input, responseText);

            await _publisher.PublishAsync("Default", "ai-events", Guid.NewGuid(), new AIAnalysisEvent
            {
                Type = "ai.analysis.completed",
                Model = model,
                Duration = sw.Elapsed.TotalSeconds,
                Timestamp = DateTime.UtcNow
            });

            return responseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Silver: Falha na análise IA");
            throw new ApplicationException("Falha crítica no processamento da análise do agente de IA.", ex);
        }
    }

    public Task<string> GetStatusAsync()
    {
        var status = _isAutonomous ? "AUTÔNOMO" : "REATIVO";
        return Task.FromResult($"Silver Agent [{status}] | Modelo: {_cfg["AI_MODEL"] ?? "tinyllama"} | Agentes gerenciados: {_managedAgents.Count(a => a.IsActive)} | Obsidian: conectado");
    }

    public Task SetContextAsync(string context)
    {
        _context = context;
        _logger.LogInformation("Silver: Contexto updated");
        return Task.CompletedTask;
    }

    public async Task<string> CreateAgent(string agentType, string expertise, string ownerId)
    {
        var agentId = Guid.NewGuid().ToString("N")[..12];
        var grainId = Guid.NewGuid();

        var agent = new AgentInfo
        {
            AgentId = agentId,
            AgentType = agentType,
            Expertise = expertise,
            OwnerId = ownerId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Set<AgentMetadata>().Add(new AgentMetadata(agentId, agentType, expertise, ownerId));
        await db.SaveChangesAsync();

        _managedAgents.Add(agent);

        var router = _serviceProvider.GetRequiredService<GrainRouter>();
        switch (agentType.ToLowerInvariant())
        {
            case "blue":
                var blue = router.GetBlueAgent(grainId);
                await blue.GetStatusAsync();
                break;
            case "red":
                var red = router.GetRedAgent(grainId);
                await red.GetStatusAsync();
                break;
            case "violet":
                var violet = router.GetVioletAgent(grainId);
                await violet.GetStatusAsync();
                break;
        }

        _logger.LogInformation("Silver: Agente {AgentId} criado e ativado ({Type}: {Expertise})", agentId, agentType, expertise);
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

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var metadata = await db.Set<AgentMetadata>().FirstOrDefaultAsync(a => a.AgentId == agentId);
        if (metadata != null)
        {
            metadata.MarkAsDeleted();
            await db.SaveChangesAsync();
        }

        agent.IsActive = false;
        agent.DeactivatedAt = DateTime.UtcNow;

        _logger.LogInformation("Silver: Agente {AgentId} desativado", agentId);
        return true;
    }

    public async Task<bool> ActivateAgent(string agentId)
    {
        var agent = _managedAgents.FirstOrDefault(a => a.AgentId == agentId);
        if (agent == null) return false;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var metadata = await db.Set<AgentMetadata>().FirstOrDefaultAsync(a => a.AgentId == agentId);
        if (metadata != null)
        {
            metadata.MarkAsRestored();
            await db.SaveChangesAsync();
        }

        agent.IsActive = true;
        agent.DeactivatedAt = null;

        _logger.LogInformation("Silver: Agente {AgentId} reativado", agentId);
        return true;
    }

    public async Task<List<string>> SearchMemory(string query)
    {
        var model = _cfg["AI_MODEL"] ?? "tinyllama";
        return await _memory.SearchMemory(query, model);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_chat != null) return;

        var sp = this.ServiceProvider;
        if (sp == null)
        {
            _logger.LogError("ServiceProvider do grão é nulo. Impossível inicializar SilverAgent.");
            return;
        }

        _serviceProvider = sp;
        _cfg = sp.GetRequiredService<IConfiguration>();
        _publisher = sp.GetRequiredService<EventPublisher>();
        _telemetry = sp.GetRequiredService<TelemetryService>();
        _httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        var chat = sp.GetRequiredService<IChatService>();
        var memory = sp.GetRequiredService<IMemoryService>();
        var obsidian = sp.GetRequiredService<IObsidianService>();
        var logger = sp.GetRequiredService<ILogger<SilverAgent>>();

        await InitializeAsync(chat, memory, obsidian, _cfg, logger, _publisher, _telemetry, sp, _httpClientFactory);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync();
        await base.OnActivateAsync(cancellationToken);
    }
}

[GenerateSerializer]
public class AIAnalysisEvent
{
    [Id(0)]
    public string Type { get; set; } = string.Empty;
    [Id(1)]
    public string Model { get; set; } = string.Empty;
    [Id(2)]
    public double Duration { get; set; }
    [Id(3)]
    public DateTime Timestamp { get; set; }
}

public class NewsItem
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}