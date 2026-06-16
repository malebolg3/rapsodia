using System.Diagnostics;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class RedAgent : Grain, IRedAgent
{
    private readonly IHttpClientFactory _http;
    private readonly IObsidianService _obsidian;
    private readonly IConfiguration _cfg;
    private readonly ILogger<RedAgent> _logger;
    private readonly EventPublisher _publisher;
    private readonly TelemetryService _telemetry;
    private int _scanCount;
    private int _exploitCount;

    public RedAgent(IHttpClientFactory http, IObsidianService obsidian, IConfiguration cfg, ILogger<RedAgent> logger, EventPublisher publisher, TelemetryService telemetry)
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
            await _obsidian.AppendNoteAsync("scans", $"### Red Scan {scanId}\n- Target: {target}\n- Type: {scanType}\n- Result: {result[..Math.Min(200, result.Length)]}\n");
            
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
            await _obsidian.AppendNoteAsync("exploits", $"### 💥 {exploitName}\n- Target: {target}\n- ID: {exploitId}\n- Result: {result[..Math.Min(200, result.Length)]}\n");
            
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
        catch
        {
            return "Red API unreachable";
        }
    }
}