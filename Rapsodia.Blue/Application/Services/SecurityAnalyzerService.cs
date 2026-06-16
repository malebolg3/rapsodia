using System.Text.Json;
using System.Net.Http.Json;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class SecurityAnalyzerService : ISecurityAnalyzer
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly bool _mock;

    public SecurityAnalyzerService(IHttpClientFactory http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
        _mock = cfg["AI_MOCK"] == "true" || string.IsNullOrEmpty(cfg["AI_URL"]);
    }

    public async Task<Result<AnalyzerResultDTO>> AnalyzeAsync(CreateAnalyzeRequest req, CancellationToken ct)
    {
        if (_mock)
            return Result<AnalyzerResultDTO>.Ok(MockResult(req));

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["AI_URL"]!);
        await client.PostAsJsonAsync("/v1/chat/completions", new
        {
            model = _cfg["AI_MODEL"] ?? "mistral",
            messages = new[] { new { role = "user", content = $"Analyze security for asset {req.AssetId} type {req.AnalysisType}" } }
        }, ct);
        
        return Result<AnalyzerResultDTO>.Ok(new AnalyzerResultDTO
        {
            Id = Random.Shared.Next(1, 1000),
            AssetId = req.AssetId,
            AnalysisType = req.AnalysisType,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        });
    }

    public Task<Result<AnalyzerResultDTO>> GetReportAsync(int id, CancellationToken ct)
        => Task.FromResult(Result<AnalyzerResultDTO>.Ok(MockResult(new CreateAnalyzeRequest { AssetId = id })));

    public Task<Result<PagedResult<AnalyzerResultDTO>>> ListReportsAsync(AnalyzerFilterDTO filter, CancellationToken ct)
        => Task.FromResult(Result<PagedResult<AnalyzerResultDTO>>.Ok(new PagedResult<AnalyzerResultDTO>
        {
            Items = new List<AnalyzerResultDTO> { MockResult(new CreateAnalyzeRequest()) },
            TotalCount = 1,
            Page = filter.Page,
            PageSize = filter.PageSize
        }));

    public Task<Result<List<AnalyzerResultDTO>>> BatchAnalyzeAsync(BatchAnalyzeRequest req, CancellationToken ct)
    {
        var results = req.AssetIds.Select(id => MockResult(new CreateAnalyzeRequest { AssetId = id, AnalysisType = req.AnalysisType })).ToList();
        return Task.FromResult(Result<List<AnalyzerResultDTO>>.Ok(results));
    }

    public Task<Result<bool>> DeleteReportAsync(int id, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public Task<Result<bool>> RestoreReportAsync(int id, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public Task<Result<AnalyzerStatsDTO>> GetStatsAsync(CancellationToken ct)
        => Task.FromResult(Result<AnalyzerStatsDTO>.Ok(new AnalyzerStatsDTO
        {
            TotalAnalyses = 128,
            CompletedAnalyses = 120,
            FailedAnalyses = 8
        }));

    public Task<AnalysisResult> AnalyzeAsync(string target, CancellationToken ct = default)
        => Task.FromResult(new AnalysisResult(target, new List<string> { "AI Analysis: Potential vulnerability found" }));

    private static AnalyzerResultDTO MockResult(CreateAnalyzeRequest req) => new()
    {
        Id = Random.Shared.Next(1, 1000),
        AssetId = req.AssetId,
        AnalysisType = req.AnalysisType,
        Status = "Completed",
        CreatedAt = DateTime.UtcNow
    };
}