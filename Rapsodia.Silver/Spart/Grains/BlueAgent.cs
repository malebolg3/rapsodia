using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public class BlueAgent : Grain, IBlueAgent
{
    private readonly IHttpClientFactory _http;
    private readonly IObsidianService _obsidian;
    private readonly IConfiguration _cfg;
    private readonly ILogger<BlueAgent> _logger;
    private readonly EventPublisher _publisher;
    private readonly TelemetryService _telemetry;
    private int _scanCount;
    private int _vulnCount;

    public BlueAgent(IHttpClientFactory http, IObsidianService obsidian, IConfiguration cfg, ILogger<BlueAgent> logger, EventPublisher publisher, TelemetryService telemetry)
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
        return Task.FromResult($"Blue Agent | Scans: {_scanCount} | Vulns: {_vulnCount}");
    }

    public async Task TrackScanAsync(string scanId, string target)
    {
        _scanCount++;
        _telemetry.RecordScan("defense");
        _logger.LogInformation("Blue: Scan {ScanId} em {Target}", scanId, target);
        
        var client = _http.CreateClient();
        var bluePort = _cfg["PORT_BLU"] ?? "5073";
        
        try
        {
            await client.PostAsJsonAsync($"http://localhost:{bluePort}/api/scan", new { scanId, target });
            await _obsidian.AppendNoteAsync("scans", $"### Scan {scanId}\n- Target: {target}\n- Time: {DateTime.UtcNow}\n");
            
            await _publisher.PublishAsync("Default", "defense-events", Guid.NewGuid(), new
            {
                type = "scan.tracked",
                scanId,
                target,
                timestamp = DateTime.UtcNow
            });
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
        var client = _http.CreateClient();
        var bluePort = _cfg["PORT_BLU"] ?? "5073";
        
        try
        {
            var response = await client.GetAsync($"http://localhost:{bluePort}/api/asset");
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
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
}