using System.Text.Json;
using System.Text;

namespace rapsodia.Services.Telemetry;

public class GeminiService(HttpClient http, IConfiguration config)
{
    private readonly string _key = config["GEMINI_KEY"]?.Trim() ?? throw new InvalidOperationException("GEMINI_KEY missing");

    public async Task<string> AnalyzeSecurityThreat(string logContent, string environment)
    {
        var payload = new
        {
            contents = new[] {
                new {
                    role = "user",
                    parts = new[] {
                        new { text = $"Você é um Principal Security Engineer. Analise APENAS ameaças de segurança usando Defense in Depth + Least Privilege.\nResponda em Português do Brasil com: Tipo de ameaça, Severidade (CRÍTICA|ALTA|MÉDIA|BAIXA), Top 3 mitigações, comandos CLI/IaC hardening.\n\n[{environment}] Log de Segurança:\n{logContent}" }
                    }
                }
            }
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        System.Diagnostics.Debug.WriteLine($"Payload: {payloadJson}");

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent")
        {
            Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-goog-api-key", _key);

        var response = await http.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        System.Diagnostics.Debug.WriteLine($"Response: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
            return $"Error: {response.StatusCode} - {responseContent}";

        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No response";
        }
        catch (Exception ex)
        {
            return $"Parse error: {ex.Message}";
        }
    }
}