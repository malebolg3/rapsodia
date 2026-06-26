// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using System.Text.Json;
using Rapsodia.Silver.Application.DTOs;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Domain.Common;

namespace Rapsodia.Silver.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _url;

    public ChatService(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        var rawUrl = Environment.GetEnvironmentVariable("AI_URL") ?? "http://ollama-server:11434";
        rawUrl = rawUrl.EndsWith('/') ? rawUrl : $"{rawUrl}/";
        _url = rawUrl.Contains("/api/") ? rawUrl : $"{rawUrl}api/";
        _model = Environment.GetEnvironmentVariable("AI_MODEL") ?? "tinyllama";
    }

    public async Task<Result<ChatResponse>> SendMessageAsync(ChatRequest request)
    {
        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "Você é um analista de cibersegurança. Responda com análise de risco." },
                new { role = "user", content = request.Message }
            },
            options = new { temperature = 0.2 },
            stream = false
        };

        var url = $"{_url}chat";

        try
        {
            using var response = await _http.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                return Result<ChatResponse>.Fail($"Falha no LLM: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                return Result<ChatResponse>.Fail("Resposta da API vazia.");
            }

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("message", out var msg) &&
                msg.TryGetProperty("content", out var text))
            {
                var content = text.GetString() ?? "Sem resposta.";

                return Result<ChatResponse>.Ok(new ChatResponse(
                    Content: content,
                    ContextEntropy: 0.5,
                    Risk: new RiskAssessment(
                        ShannonEntropy: 0.5,
                        RiskScore: 0.3,
                        SeverityLevel: "Low",
                        RiskNarrative: new List<string> { "Análise automática" },
                        VulnerabilityChains: new List<ChainedVulnerability>()
                    ),
                    RecommendedTools: new List<string>(),
                    Timestamp: DateTime.UtcNow
                ));
            }

            return Result<ChatResponse>.Fail("Estrutura de resposta inválida.");
        }
        catch (Exception ex)
        {
            return Result<ChatResponse>.Fail($"Erro na comunicação com o LLM: {ex.Message}");
        }
    }

    public Task<Result<ConversationSummary>> GetConversationAsync(Guid conversationId)
        => Task.FromResult(Result<ConversationSummary>.Fail("Não implementado."));

    public Task<PagedResult<ConversationSummary>> ListConversationsAsync(Guid userId, int page = 1, int pageSize = 20)
        => Task.FromResult(new PagedResult<ConversationSummary>
        {
            Items = new List<ConversationSummary>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        });
}