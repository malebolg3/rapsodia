using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Orleans;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class SilverAgent : Grain, ISilverAgent
{
    private string _context = "Você é um analista de cibersegurança especializado em identificar vulnerabilidades e sugerir remediações.";
    private readonly HttpClient _aiClient;
    private readonly IObsidianService _obsidian;
    private readonly IConfiguration _cfg;
    private readonly ILogger<SilverAgent> _logger;
    private readonly EventPublisher _publisher;
    private readonly TelemetryService _telemetry;

    public SilverAgent(IHttpClientFactory httpFactory, IObsidianService obsidian, IConfiguration cfg, ILogger<SilverAgent> logger, EventPublisher publisher, TelemetryService telemetry)
    {
        _aiClient = httpFactory.CreateClient("AIAgent");
        _obsidian = obsidian;
        _cfg = cfg;
        _logger = logger;
        _publisher = publisher;
        _telemetry = telemetry;
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
            
            await _obsidian.AppendNoteAsync("ai-analysis", $"## Análise\n**Modelo:** {model}\n**Duração:** {sw.Elapsed.TotalSeconds:F2}s\n**Input:** {input[..Math.Min(100, input.Length)]}...\n\n```json\n{result}\n```\n---\n");
            
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
        return Task.FromResult($"Silver Agent Ativo | Modelo: {_cfg["AI_MODEL"] ?? "mistral"} | Obsidian: conectado");
    }

    public Task SetContextAsync(string context)
    {
        _context = context;
        _logger.LogInformation("Silver: Contexto atualizado");
        return Task.CompletedTask;
    }
}