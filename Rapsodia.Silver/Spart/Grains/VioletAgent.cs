using System.Diagnostics;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class VioletAgent : Grain, IVioletAgent
{
    private readonly IHttpClientFactory _http;
    private readonly IObsidianService _obsidian;
    private readonly IConfiguration _cfg;
    private readonly ILogger<VioletAgent> _logger;
    private readonly EventPublisher _publisher;
    private readonly TelemetryService _telemetry;
    private int _labCount;
    private readonly List<LabInfo> _activeLabs = new();

    public VioletAgent(IHttpClientFactory http, IObsidianService obsidian, IConfiguration cfg, ILogger<VioletAgent> logger, EventPublisher publisher, TelemetryService telemetry)
    {
        _http = http;
        _obsidian = obsidian;
        _cfg = cfg;
        _logger = logger;
        _publisher = publisher;
        _telemetry = telemetry;
    }

    public Task<string> GetStatusAsync()
    {
        var byLevel = _activeLabs.GroupBy(l => l.Level).ToDictionary(g => g.Key, g => g.Count());
        return Task.FromResult($"Violet Agent | Labs: {_labCount} | Por nível: {string.Join(", ", byLevel.Select(kv => $"N{kv.Key}={kv.Value}"))}");
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
        var image = honeypotType switch
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

        var ports = honeypotType switch
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
        var scenarios = new Dictionary<string, string>
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
        _labCount--;
        _activeLabs.RemoveAll(l => l.Id == labId);
        
        var client = _http.CreateClient();
        var vltPort = _cfg["PORT_VLT"] ?? "5075";

        try
        {
            await client.DeleteAsync($"http://localhost:{vltPort}/api/lab/{labId}");
            await _obsidian.AppendNoteAsync("labs", $"### 🗑️ Lab {labId} destroyed at {DateTime.UtcNow}\n");
            
            await _publisher.PublishAsync("Default", "lab-events", Guid.NewGuid(), new
            {
                type = "lab.destroyed",
                labId,
                timestamp = DateTime.UtcNow
            });
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetLabStatusAsync(string labId)
    {
        var client = _http.CreateClient();
        var vltPort = _cfg["PORT_VLT"] ?? "5075";

        try
        {
            var response = await client.GetAsync($"http://localhost:{vltPort}/api/lab/{labId}");
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return "Violet API unreachable";
        }
    }

    public Task<List<LabInfo>> ListLabsAsync()
    {
        return Task.FromResult(_activeLabs);
    }

    public async Task<int> CleanupExpiredAsync()
    {
        var expired = _activeLabs.Where(l => l.ExpiresAt < DateTime.UtcNow).ToList();
        foreach (var lab in expired)
        {
            await DestroyLabAsync(lab.Id);
        }
        return expired.Count;
    }

    private async Task<string> CreateEnvironmentAsync(string name, string image, int level, int ttlMinutes,
        Dictionary<string, string>? envVars = null, List<string>? ports = null)
    {
        _labCount++;
        var client = _http.CreateClient();
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

            var result = await response.Content.ReadFromJsonAsync<LabCreateResponse>();
            labId = result?.ContainerId ?? labId;

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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Violet: Falha ao criar ambiente");
        }

        return labId;
    }
}

public class LabCreateResponse
{
    public string ContainerId { get; set; } = string.Empty;
}

public class LabInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Image { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}