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

public class VioletAgent : HybridAgent, IVioletAgent
{
    private readonly IHttpClientFactory _http;
    private IConfiguration _cfg = null!;
    private EventPublisher _publisher = null!;
    private TelemetryService _telemetry = null!;
    private int _labCount;
    private readonly List<LabInfo> _activeLabs = new();

    public VioletAgent(IHttpClientFactory http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public Task InitializeAsync(
        IChatService chat,
        IMemoryService memory,
        IObsidianService obsidian,
        IConfiguration cfg,
        ILogger<VioletAgent> logger,
        EventPublisher publisher,
        TelemetryService telemetry)
    {
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

        Initialize(
            chat, memory, obsidian, logger,
            agentName: "VioletAgent",
            agentColor: "violet",
            context: "Você é um orquestrador de ambientes isolados de cibersegurança. Gerencie laboratórios, honeypots e cyber ranges. Otimize recursos e sugira novos ambientes baseado em necessidades de teste.",
            startAutonomous: true
        );

        return Task.CompletedTask;
}

    protected override async Task AutonomousTick()
    {
        var expired = await CleanupExpiredAsync();
        if (expired > 0)
        {
            _logger.LogInformation("Violet: {Count} labs expirados removidos", expired);
        }

        if (_activeLabs.Count < 3)
        {
            var decision = await _chat.SendMessageAsync(new ChatRequest(
                ConversationId: Guid.NewGuid(),
                Message: $"Existem {_activeLabs.Count} laboratórios ativos. É recomendado criar mais? Responda com 'SIM: <tipo>' ou 'NAO'."
            ));

            var response = decision.Data?.Content ?? "";
            if (response.StartsWith("SIM:", StringComparison.OrdinalIgnoreCase))
            {
                await CreateLabAsync("auto-lab", "kalilinux/kali-rolling:latest");
                _logger.LogInformation("Violet: Auto-provisionamento de lab");
            }
        }
    }

    public Task<string> GetStatusAsync()
    {
        var status = _isAutonomous ? "AUTÔNOMO" : "REATIVO";
        var byLevel = _activeLabs.GroupBy(l => l.Level).ToDictionary(g => g.Key, g => g.Count());
        return Task.FromResult($"Violet Agent [{status}] | Labs: {_labCount} | Por nível: {string.Join(", ", byLevel.Select(kv => $"N{kv.Key}={kv.Value}"))}");
    }

    public async Task<string> CreateLabAsync(string name, string image)
    {
        return await CreateEnvironmentAsync(name, image, 1, 60);
    }

    public async Task<string> CreateSandboxAsync(string name, string image, List<string> tools, int ttlMinutes = 240)
    {
        var envVars = new Dictionary<string, string>
        {
            ["TOOLS"] = string.Join(",", tools),
            ["MODE"] = "sandbox"
        };
        var labId = await CreateEnvironmentAsync(name, image, 2, ttlMinutes, envVars);

        await _publisher.PublishAsync("Default", "lab-events", Guid.NewGuid(), new
        {
            type = "sandbox.created",
            labId,
            name,
            tools,
            timestamp = DateTime.UtcNow
        });

        return labId;
    }

    public async Task<string> DeployHoneypotAsync(string name, string honeypotType, int ttlMinutes = 480)
    {
        var image = honeypotType.ToLowerInvariant() switch
        {
            "ssh" => "cowrie/cowrie:latest",
            "http" => "nginx:alpine",
            "ics" => "mushorg/conpot:latest",
            "dionaea" => "dinotools/dionaea:latest",
            _ => "cowrie/cowrie:latest"
        };

        var envVars = new Dictionary<string, string>
        {
            ["HONEYPOT_TYPE"] = honeypotType,
            ["LOG_LEVEL"] = "debug"
        };

        var ports = honeypotType.ToLowerInvariant() switch
        {
            "ssh" => new List<string> { "2222:22" },
            "http" => new List<string> { "8080:80" },
            "ics" => new List<string> { "502:502", "102:102" },
            _ => new List<string>()
        };

        var labId = await CreateEnvironmentAsync(name, image, 3, ttlMinutes, envVars, ports);

        await _obsidian.AppendNoteAsync("honeypots", $"""
### 🍯 Honeypot: {name}
- **Tipo:** {honeypotType}
- **ID:** {labId}
- **Imagem:** {image}
- **Portas:** {string.Join(", ", ports)}
- **Criado:** {DateTime.UtcNow}
---
""");

        await _publisher.PublishAsync("Default", "lab-events", Guid.NewGuid(), new
        {
            type = "honeypot.deployed",
            labId,
            name,
            honeypotType,
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Honeypot {Name} ({Type}) implantado em {LabId}", name, honeypotType, labId);
        return labId;
    }

    public async Task<string> DeploySOCAsync(string name, int ttlMinutes = 480)
    {
        var envVars = new Dictionary<string, string>
        {
            ["ELASTIC_PASSWORD"] = Guid.NewGuid().ToString("N")[..16],
            ["WAZUH_VERSION"] = "4.7",
            ["MODE"] = "soc"
        };

        var labId = await CreateEnvironmentAsync(name, "wazuh/wazuh:latest", 4, ttlMinutes, envVars,
            new List<string> { "5601:5601", "9200:9200", "1514:1514/udp", "1515:1515", "55000:55000" });

        await _obsidian.AppendNoteAsync("soc", $"""
### 🛡️ SOC: {name}
- **ID:** {labId}
- **Wazuh Dashboard:** http://localhost:5601
- **Elasticsearch:** http://localhost:9200
- **Agente porta:** 1514/udp
- **Senha Elastic:** {envVars["ELASTIC_PASSWORD"]}
- **Criado:** {DateTime.UtcNow}
- **Expira:** {DateTime.UtcNow.AddMinutes(ttlMinutes)}
---
""");

        await _publisher.PublishAsync("Default", "lab-events", Guid.NewGuid(), new
        {
            type = "soc.deployed",
            labId,
            name,
            dashboard = "http://localhost:5601",
            timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("SOC {Name} implantado em {LabId}", name, labId);
        return labId;
    }

    public async Task<string> DeployCyberRangeAsync(string name, string scenario, int ttlMinutes = 1440)
    {
        var scenarios = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ransomware"] = "kalilinux/kali-rolling:latest,ubuntu:22.04,windows:servercore",
            ["phishing"] = "kalilinux/kali-rolling:latest,ubuntu:22.04,mailhog/mailhog:latest",
            ["exfiltration"] = "kalilinux/kali-rolling:latest,ubuntu:22.04,mysql:8,redis:alpine",
            ["ddos"] = "kalilinux/kali-rolling:latest,nginx:alpine,ubuntu:22.04,ubuntu:22.04"
        };

        var images = scenarios.GetValueOrDefault(scenario, "kalilinux/kali-rolling:latest,ubuntu:22.04");

        var envVars = new Dictionary<string, string>
        {
            ["SCENARIO"] = scenario,
            ["RANGE_ID"] = Guid.NewGuid().ToString("N")[..8]
        };

        var labId = await CreateEnvironmentAsync(name, images, 5, ttlMinutes, envVars,
            new List<string> { "8080:80", "4444:4444", "2222:22" });

        await _obsidian.AppendNoteAsync("cyber-ranges", $"""
### 🎯 Cyber Range: {name}
- **ID:** {labId}
- **Cenário:** {scenario}
- **Containers:** {images}
- **Criado:** {DateTime.UtcNow}
- **Expira:** {DateTime.UtcNow.AddMinutes(ttlMinutes)}
---
""");

        await _publisher.PublishAsync("Default", "lab-events", Guid.NewGuid(), new
        {
            type = "cyber-range.deployed",
            labId,
            name,
            scenario,
            timestamp = DateTime.UtcNow
        });

        _logger.LogWarning("Cyber Range {Name} ({Scenario}) implantado em {LabId}", name, scenario, labId);
        return labId;
    }

    public async Task<bool> DestroyLabAsync(string labId)
    {
        var client = _http.CreateClient("VioletAgentClient");
        var vltPort = _cfg["PORT_VLT"] ?? "5075";

        try
        {
            var response = await client.DeleteAsync($"http://localhost:{vltPort}/api/lab/{labId}");
            if (!response.IsSuccessStatusCode) return false;

            _labCount--;
            _activeLabs.RemoveAll(l => l.Id == labId);
            
            await _obsidian.AppendNoteAsync("labs", $"### 🗑️ Lab {labId} destroyed at {DateTime.UtcNow}\n");
            await _publisher.PublishAsync("Default", "lab-events", Guid.NewGuid(), new
            {
                type = "lab.destroyed",
                labId,
                timestamp = DateTime.UtcNow
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Violet: Erro ao destruir lab {LabId}", labId);
            return false;
        }
    }

    public async Task<string> GetLabStatusAsync(string labId)
    {
        var client = _http.CreateClient("VioletAgentClient");
        var vltPort = _cfg["PORT_VLT"] ?? "5075";

        try
        {
            var response = await client.GetAsync($"http://localhost:{vltPort}/api/lab/{labId}");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Violet: Erro ao obter status do lab {LabId}", labId);
            return "Violet API unreachable";
        }
    }

    public Task<List<LabInfo>> ListLabsAsync()
    {
        return Task.FromResult(_activeLabs.ToList());
    }

    public async Task<int> CleanupExpiredAsync()
    {
        var expired = _activeLabs.Where(l => l.ExpiresAt < DateTime.UtcNow).ToList();
        int successCount = 0;
        
        foreach (var lab in expired)
        {
            if (await DestroyLabAsync(lab.Id))
            {
                successCount++;
            }
        }
        return successCount;
    }

    private async Task<string> CreateEnvironmentAsync(string name, string image, int level, int ttlMinutes,
        Dictionary<string, string>? envVars = null, List<string>? ports = null)
    {
        var client = _http.CreateClient("VioletAgentClient");
        var vltPort = _cfg["PORT_VLT"] ?? "5075";
        var labId = Guid.NewGuid().ToString("N")[..12];

        _telemetry.RecordLabCreated(level);

        try
        {
            var response = await client.PostAsJsonAsync($"http://localhost:{vltPort}/api/lab", new
            {
                name,
                image,
                ttlMinutes,
                envVars,
                ports
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LabCreateResponse>();
                labId = result?.ContainerId ?? labId;

                _labCount++;
                _activeLabs.Add(new LabInfo
                {
                    Id = labId,
                    Name = name,
                    Level = level,
                    Image = image,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes)
                });

                var levelLabel = level switch { 1 => "Básico", 2 => "Sandbox", 3 => "Honeypot", 4 => "SOC", 5 => "Cyber Range", _ => "Custom" };
                _logger.LogInformation("Violet: {Level} '{Name}' criado ({LabId})", levelLabel, name, labId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Violet: Falha ao criar ambiente");
        }

        return labId;
    }
}

[GenerateSerializer]
public class LabCreateResponse
{
    [Id(0)]
    public string ContainerId { get; set; } = string.Empty;
}

[GenerateSerializer]
public class LabInfo
{
    [Id(0)]
    public string Id { get; set; } = string.Empty;
    [Id(1)]
    public string Name { get; set; } = string.Empty;
    [Id(2)]
    public int Level { get; set; }
    [Id(3)]
    public string Image { get; set; } = string.Empty;
    [Id(4)]
    public DateTime CreatedAt { get; set; }
    [Id(5)]
    public DateTime ExpiresAt { get; set; }
}