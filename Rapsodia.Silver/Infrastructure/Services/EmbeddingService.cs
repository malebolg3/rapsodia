// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using System.Text.Json;

namespace Rapsodia.Silver.Infrastructure.Services;

public class EmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _url;

    public EmbeddingService(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        var baseEnv = Environment.GetEnvironmentVariable("AI_URL") ?? "http://172.18.0.1:11434";
        _url = baseEnv.EndsWith('/') ? baseEnv : $"{baseEnv}/";
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var payload = new { model = "nomic-embed-text", prompt = text };
        using var response = await _http.PostAsJsonAsync($"{_url}embeddings", payload);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        
        if (!doc.RootElement.TryGetProperty("embedding", out var embeddingProp))
            throw new KeyNotFoundException("Propriedade 'embedding' ausente no payload de resposta.");

        return embeddingProp.EnumerateArray().Select(e => e.GetSingle()).ToArray();
    }

    public static float CosineSimilarity(float[] a, float[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return 0f;
        float dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        if (normA == 0 || normB == 0) return 0f;
        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}