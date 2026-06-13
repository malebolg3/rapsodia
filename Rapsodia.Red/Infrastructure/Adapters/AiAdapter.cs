using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Rapsodia.Red.Domain.Interfaces;

namespace Rapsodia.Red.Infrastructure.Adapters;

public sealed class AiAdapter : IAiAnalystPort
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly bool _mock;

    public AiAdapter(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
        _mock = cfg["AI_MOCK"] == "true" || cfg["KALI_MOCK"] == "true" || string.IsNullOrEmpty(cfg["AI_URL"]);
    }

    public async Task<string> AnalyzeScanResultsAsync(string toolName, string rawOutput)
    {
        if (_mock)
        {
            await Task.CompletedTask;
            return $"{{\"tool\": \"{toolName}\", \"findings\": [{{\"severity\": \"high\", \"cve\": \"CVE-2024-MOCK\", \"description\": \"Mock analysis result\"}}], \"risk_level\": \"medium\"}}";
        }

        var model = _cfg["AI_MODEL"] ?? "mistral";
        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = "Analyze tool logs and extract critical vulnerabilities. Respond in JSON format." },
                new { role = "user", content = $"Tool: {toolName}\nData:\n{rawOutput}" }
            }
        };

        var response = await _http.PostAsJsonAsync("/v1/chat/completions", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}