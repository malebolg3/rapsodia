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

    public ChatService()
    {
        var rawUrl = Environment.GetEnvironmentVariable("AI_URL") ?? "http://172.18.0.1:11434";
        _url = rawUrl.TrimEnd('/');
        _model = Environment.GetEnvironmentVariable("AI_MODEL") ?? "phi3:latest";
        _http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    public async Task<Result<ChatResponse>> SendMessageAsync(ChatRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var payload = new { model = _model, prompt = request.Message, stream = false };
        var endpoint = $"{_url}/api/generate";

        Console.WriteLine($"[ChatService] Enviando para: {endpoint}");
        Console.WriteLine($"[ChatService] Modelo: {_model}");
        Console.WriteLine($"[ChatService] Prompt: {request.Message[..Math.Min(50, request.Message.Length)]}...");

        try
        {
            using var response = await _http.PostAsJsonAsync(endpoint, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result<ChatResponse>.Fail($"LLM failure: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
                return Result<ChatResponse>.Fail("Empty API response content.");

            Console.WriteLine($"[ChatService] Resposta crua: {json[..Math.Min(200, json.Length)]}...");

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("response", out var text))
                return Result<ChatResponse>.Fail("Invalid json structure template missing response property.");

            return Result<ChatResponse>.Ok(new ChatResponse(
                Content: text.GetString() ?? "Sem resposta.",
                ContextEntropy: 0.5,
                Risk: new RiskAssessment(
                    ShannonEntropy: 0.5,
                    RiskScore: 0.3,
                    SeverityLevel: "Low",
                    RiskNarrative: ["Análise automática"],
                    VulnerabilityChains: []
                ),
                RecommendedTools: [],
                Timestamp: DateTime.UtcNow
            ));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] ERRO: {ex.Message}");
            return Result<ChatResponse>.Fail($"LLM communication transit error: {ex.Message}");
        }
    }

    public Task<Result<ConversationSummary>> GetConversationAsync(Guid conversationId)
        => Task.FromResult(Result<ConversationSummary>.Fail("Não implementado."));

    public Task<PagedResult<ConversationSummary>> ListConversationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var activePage = page <= 0 ? 1 : page;
        var activePageSize = pageSize <= 0 ? 20 : pageSize;

        return Task.FromResult(new PagedResult<ConversationSummary>
        {
            Items = [],
            TotalCount = 0,
            Page = activePage,
            PageSize = activePageSize
        });
    }
}