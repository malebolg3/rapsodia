using System.Net.Http.Json;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class ReportService : IReportService
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly bool _mock;

    public ReportService(IHttpClientFactory http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
        _mock = cfg["DOC_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DOC_URL"]);
    }

    public async Task<Result<ReportResultDTO>> GenerateAsync(CreateReportRequest req, CancellationToken ct)
    {
        if (_mock)
            return Result<ReportResultDTO>.Ok(MockReport(req));

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        var res = await client.PostAsJsonAsync("/api/report/generate", req, ct);
        var data = await res.Content.ReadFromJsonAsync<ReportResultDTO>(cancellationToken: ct);
        return Result<ReportResultDTO>.Ok(data!);
    }

    public async Task<Result<ReportResultDTO>> GetByIdAsync(int id, CancellationToken ct)
    {
        if (_mock)
            return Result<ReportResultDTO>.Ok(MockReport(new CreateReportRequest()));

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        var data = await client.GetFromJsonAsync<ReportResultDTO>($"/api/report/{id}", ct);
        return data is null ? Result<ReportResultDTO>.Fail("Report not found") : Result<ReportResultDTO>.Ok(data);
    }

    public async Task<Result<PagedResult<ReportResultDTO>>> ListAsync(ReportFilterDTO filter, CancellationToken ct)
    {
        if (_mock)
            return Result<PagedResult<ReportResultDTO>>.Ok(new PagedResult<ReportResultDTO>
            {
                Items = new List<ReportResultDTO> { MockReport(new CreateReportRequest()) },
                TotalCount = 1,
                Page = filter.Page,
                PageSize = filter.PageSize
            });

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        var data = await client.GetFromJsonAsync<PagedResult<ReportResultDTO>>(
            $"/api/report?page={filter.Page}&pageSize={filter.PageSize}", ct);
        return Result<PagedResult<ReportResultDTO>>.Ok(data!);
    }

    public async Task<Result<ReportFileDTO>> ExportPdfAsync(int id, CancellationToken ct)
    {
        if (_mock)
        {
            var pdf = "Mock PDF Content"u8.ToArray();
            return Result<ReportFileDTO>.Ok(new ReportFileDTO
            {
                Content = pdf,
                FileName = $"report_{id}.pdf",
                ContentType = "application/pdf"
            });
        }

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        var bytes = await client.GetByteArrayAsync($"/api/report/{id}/export/pdf", ct);
        return Result<ReportFileDTO>.Ok(new ReportFileDTO
        {
            Content = bytes,
            FileName = $"report_{id}.pdf",
            ContentType = "application/pdf"
        });
    }

    public async Task<Result<ReportResultDTO>> ExportJsonAsync(int id, CancellationToken ct)
    {
        if (_mock)
            return Result<ReportResultDTO>.Ok(MockReport(new CreateReportRequest { Format = "json" }));

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        var data = await client.GetFromJsonAsync<ReportResultDTO>($"/api/report/{id}/export/json", ct);
        return Result<ReportResultDTO>.Ok(data!);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        if (_mock) return Result<bool>.Ok(true);

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        await client.DeleteAsync($"/api/report/{id}", ct);
        return Result<bool>.Ok(true);
    }

    public Task<Result<bool>> RestoreAsync(int id, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public async Task<Result<DashboardDTO>> GetDashboardAsync(CancellationToken ct)
    {
        if (_mock)
            return Result<DashboardDTO>.Ok(MockDashboard());

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_cfg["DOC_URL"]!);
        client.DefaultRequestHeaders.Add("X-API-Key", _cfg["DOC_KEY"]);
        var data = await client.GetFromJsonAsync<DashboardDTO>("/api/dashboard", ct);
        return Result<DashboardDTO>.Ok(data!);
    }

    private static ReportResultDTO MockReport(CreateReportRequest req) => new()
    {
        Id = Random.Shared.Next(1, 1000),
        ReportType = req.ReportType,
        Status = "Generated",
        Format = req.Format ?? "pdf",
        GeneratedAt = DateTime.UtcNow,
        Data = new Dictionary<string, object> { ["findings"] = 42, ["severity"] = "high" }
    };

    private static DashboardDTO MockDashboard() => new()
    {
        TotalAssets = 156,
        TotalVulnerabilities = 423,
        ActiveIncidents = 7,
        CompletedScans = 89,
        VulnsBySeverity = new Dictionary<string, int> { ["Critical"] = 12, ["High"] = 45, ["Medium"] = 134, ["Low"] = 232 },
        RecentActivities = new List<RecentActivityDTO>
        {
            new() { ActivityType = "scan", Description = "Network scan completed", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new() { ActivityType = "vuln", Description = "Critical CVE detected", Timestamp = DateTime.UtcNow.AddHours(-3) }
        }
    };
}