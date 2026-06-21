// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Rapsodia.Silver.Infrastructure.Services;

public class AiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _url;

    public AiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _url = Environment.GetEnvironmentVariable("AI_URL") 
            ?? throw new InvalidOperationException("AI_URL nao configurada.");
        _model = Environment.GetEnvironmentVariable("AI_MODEL") 
            ?? throw new InvalidOperationException("AI_MODEL nao configurado.");
        
        _httpClient.BaseAddress = new Uri(_url);
    }

    public async Task<string?> GenerateAsync(string prompt)
    {
        var payload = new { model = _model, prompt, stream = false };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_httpClient.BaseAddress + "generate", content);
        
        if (!response.IsSuccessStatusCode) return null;
        
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("response", out var resp) ? resp.GetString() : null;
    }
}