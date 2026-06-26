// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Rapsodia.Silver.Application.DTOs;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class BlueAgent : HybridAgent, IBlueAgent
{
    private readonly IHttpClientFactory _http;
    private IConfiguration _cfg = null!;
    private EventPublisher _publisher = null!;
    private TelemetryService _telemetry = null!;
    private int _scanCount;
    private int _vulnCount;

    public BlueAgent(IHttpClientFactory http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public Task InitializeAsync(
        IChatService chat,
        IMemoryService memory,
        IObsidianService obsidian,
        IConfiguration cfg,
        ILogger<BlueAgent> logger,
        EventPublisher publisher,
        TelemetryService telemetry)
    {
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

        Initialize(
            chat, memory, obsidian, logger,
            agentName: "BlueAgent",
            agentColor: "blue",
            context: "Você é um especialista em DEFESA cibernética. Foque em mitigação, patches, hardening, detecção de intrusões e resposta a incidentes. Analise ameaças sob a ótica de proteção.",
            startAutonomous: true
        );
        return Task.CompletedTask;
    }

    protected override async Task AutonomousTick()
    {
        try
        {
            var client = _http.CreateClient("BlueAgentClient");
            var bluePort = _cfg["PORT_BLU"] ?? "5073";

            var response = await client.GetAsync($"http://localhost:{bluePort}/api/asset");
            if (response.IsSuccessStatusCode)
            {
                var assets = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(assets) && assets.Length > 10)
                {
                    var analysis = await _chat.SendMessageAsync(new ChatRequest(
                        ConversationId: Guid.NewGuid(),
                        Message: $"Analise estes ativos de rede em busca de vulnerabilidades e sugira ações defensivas: {assets[..Math.Min(500, assets.Length)]}"
                    ));

                    if (analysis.Data?.Content != null)
                    {
                        await _obsidian.AppendNoteAsync("defense", $"## 🛡️ Análise Defensiva Automática\n{analysis.Data.Content}\n---\n{DateTime.UtcNow}\n");
                        _logger.LogInformation("Blue: Análise defensiva concluída");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Blue: Erro no tick autônomo");
        }
    }

    public Task<string> GetStatusAsync()
    {
        var status = _isAutonomous ? "AUTÔNOMO" : "REATIVO";
        return Task.FromResult($"Blue Agent [{status}] | Scans: {_scanCount} | Vulns: {_vulnCount}");
    }

    public async Task TrackScanAsync(string scanId, string target)
    {
        var client = _http.CreateClient("BlueAgentClient");
        var bluePort = _cfg["PORT_BLU"] ?? "5073";

        try
        {
            var response = await client.PostAsJsonAsync($"http://localhost:{bluePort}/api/scan", new { scanId, target });
            if (response.IsSuccessStatusCode)
            {
                _scanCount++;
                _telemetry.RecordScan("defense");
                _logger.LogInformation("Blue: Scan {ScanId} em {Target}", scanId, target);

                await _obsidian.AppendNoteAsync("scans", $"### Scan {scanId}\n- Target: {target}\n- Time: {DateTime.UtcNow}\n");
                await _publisher.PublishAsync("Default", "defense-events", Guid.NewGuid(), new
                {
                    type = "scan.tracked",
                    scanId,
                    target,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Blue: Falha ao comunicar com API Blue");
        }
    }

    public async Task TrackVulnAsync(string vulnId, string severity)
    {
        _vulnCount++;
        _logger.LogInformation("Blue: Vuln {VulnId} severidade {Severity}", vulnId, severity);
        await _obsidian.AppendNoteAsync("vulnerabilities", $"### {vulnId}\n- Severity: {severity}\n- Time: {DateTime.UtcNow}\n");
    }

    public async Task<string> GetAssetSummaryAsync()
    {
        var client = _http.CreateClient("BlueAgentClient");
        var bluePort = _cfg["PORT_BLU"] ?? "5073";

        try
        {
            var response = await client.GetAsync($"http://localhost:{bluePort}/api/asset");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blue API unreachable");
            return "Blue API unreachable";
        }
    }

    public async Task NotifyIncidentAsync(string incidentId, string title, string severity)
    {
        _logger.LogWarning("Blue: Incidente {IncidentId} - {Title} [{Severity}]", incidentId, title, severity);
        await _obsidian.AppendNoteAsync("incidents", $"## 🚨 {title}\n- ID: {incidentId}\n- Severity: **{severity}**\n- Time: {DateTime.UtcNow}\n---\n");

        await _publisher.PublishAsync("Default", "defense-events", Guid.NewGuid(), new
        {
            type = "incident.created",
            incidentId,
            title,
            severity,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task<string> AnalyzeThreatAsync(string threatDescription)
    {
        var result = await _chat.SendMessageAsync(new ChatRequest(
            ConversationId: Guid.NewGuid(),
            Message: $"Analise esta ameaça de segurança sob a ótica defensiva: {threatDescription}"
        ));
        return result.Data?.Content ?? "Análise indisponível.";
    }
}