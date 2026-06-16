using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Rapsodia.Red.Infrastructure.Reporting;

public class ObsidianReportService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ObsidianReportService> _logger;
    private readonly bool _mock;

    public ObsidianReportService(HttpClient http, IConfiguration cfg, ILogger<ObsidianReportService> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
        _mock = cfg["DOC_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DOC_URL"]);
    }

    public async Task<string> GenerateReportAsync(string title, Dictionary<string, object> data)
    {
        if (_mock)
        {
            await Task.Delay(100);
            return $"# {title}\n\n## Findings\n- Critical: 2\n- High: 5\n- Medium: 12\n\n## Target\n- 127.0.0.1\n\n## AI Analysis\nAll vulnerabilities identified and categorized.";
        }

        var docUrl = _cfg["DOC_URL"]!;
        var apiKey = _cfg["DOC_KEY"];
        
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        
        var payload = new
        {
            title,
            content = JsonSerializer.Serialize(data),
            type = "pentest-report",
            timestamp = DateTime.UtcNow
        };

        var response = await _http.PostAsJsonAsync($"{docUrl}/api/notes", payload);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> AppendFindingAsync(string reportId, string finding)
    {
        if (_mock) return true;
        
        var docUrl = _cfg["DOC_URL"]!;
        var apiKey = _cfg["DOC_KEY"];
        
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        
        var response = await _http.PostAsJsonAsync($"{docUrl}/api/notes/{reportId}/append", new { content = finding });
        return response.IsSuccessStatusCode;
    }
}