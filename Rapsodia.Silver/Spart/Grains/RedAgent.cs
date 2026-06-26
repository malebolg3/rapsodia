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

public class RedAgent : Grain, IRedAgent
{
    private readonly IHttpClientFactory _http;
    private readonly IObsidianService _obsidian;
    private readonly IChatService _chat;
    private readonly IConfiguration _cfg;
    private readonly ILogger<RedAgent> _logger;
    private readonly EventPublisher _publisher;
    private readonly TelemetryService _telemetry;
    private int _scanCount;
    private int _exploitCount;

    public RedAgent(IHttpClientFactory http, IObsidianService obsidian, IChatService chat, IConfiguration cfg, ILogger<RedAgent> logger, EventPublisher publisher, TelemetryService telemetry)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _obsidian = obsidian ?? throw new ArgumentNullException(nameof(obsidian));
        _chat = chat ?? throw new ArgumentNullException(nameof(chat));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
    }

    public Task<string> GetStatusAsync()
    {
        return Task.FromResult($"Red Agent | Scans: {_scanCount} | Exploits: {_exploitCount}");
    }

    public async Task<string> StartScanAsync(string target, string scanType)
    {
        _scanCount++;
        var client = _http.CreateClient();
        var redPort = _cfg["PORT_RED"] ?? "5074";
        var scanId = Guid.NewGuid().ToString("N")[..8];
        
        _telemetry.RecordScan(scanType);

        try
        {
            var response = await client.PostAsJsonAsync($"http://localhost:{redPort}/api/scan", new
            {
                target,
                scanType,
                ports = new[] { 22, 80, 443, 3306, 8080 }
            });

            var result = await response.Content.ReadAsStringAsync();
            var slicedResult = result[..Math.Min(200, result.Length)];
            await _obsidian.AppendNoteAsync("scans", $"### Red Scan {scanId}\n- Target: {target}\n- Type: {scanType}\n- Result: {slicedResult}\n");
            
            await _publisher.PublishAsync("Default", "attack-events", Guid.NewGuid(), new
            {
                type = "scan.started",
                scanId,
                target,
                scanType,
                timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation("Red: Scan {ScanId} iniciado em {Target}", scanId, target);
            return scanId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Red: Falha ao iniciar scan");
            return $"mock-{scanId}";
        }
    }

    public async Task<string> StartExploitAsync(string target, string exploitName, Dictionary<string, object>? payload)
    {
        _exploitCount++;
        var client = _http.CreateClient();
        var redPort = _cfg["PORT_RED"] ?? "5074";
        var exploitId = Guid.NewGuid().ToString("N")[..8];
        
        _telemetry.RecordExploit(exploitName);

        try
        {
            var response = await client.PostAsJsonAsync($"http://localhost:{redPort}/api/exploit", new
            {
                target,
                exploitName,
                payload
            });

            var result = await response.Content.ReadAsStringAsync();
            var slicedResult = result[..Math.Min(200, result.Length)];
            await _obsidian.AppendNoteAsync("exploits", $"### {exploitName}\n- Target: {target}\n- ID: {exploitId}\n- Result: {slicedResult}\n");
            
            await _publisher.PublishAsync("Default", "attack-events", Guid.NewGuid(), new
            {
                type = "exploit.executed",
                exploitId,
                target,
                exploitName,
                timestamp = DateTime.UtcNow
            });
            
            _logger.LogWarning("Red: Exploit {ExploitName} em {Target}", exploitName, target);
            return exploitId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Red: Falha ao executar exploit");
            return $"mock-{exploitId}";
        }
    }

    public async Task<string> GetScanResultAsync(string scanId)
    {
        var client = _http.CreateClient();
        var redPort = _cfg["PORT_RED"] ?? "5074";
        
        try
        {
            var response = await client.GetAsync($"http://localhost:{redPort}/api/scan/{scanId}");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Red API unreachable");
            return "Red API unreachable";
        }
    }

    public async Task<string> AnalyzeTargetAsync(string target)
    {
        var result = await _chat.SendMessageAsync(new ChatRequest(
            ConversationId: Guid.NewGuid(),
            Message: $"Analise o alvo {target} para possíveis vulnerabilidades e sugira o melhor exploit."
        ));
        return result.Data?.Content ?? "Análise indisponível.";
    }
}