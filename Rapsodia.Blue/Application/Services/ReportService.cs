// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class ReportService : IReportService
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;

    public ReportService(IHttpClientFactory http, IConfiguration cfg)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
    }

    public async Task<Result<ReportResultDTO>> GenerateAsync(CreateReportRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        try
        {
            var client = CreateConfiguredClient();
            var res = await client.PostAsJsonAsync("/api/report/generate", req, ct);
            if (!res.IsSuccessStatusCode)
                return Result<ReportResultDTO>.Fail($"Failed to generate report: {res.StatusCode}");

            var data = await res.Content.ReadFromJsonAsync<ReportResultDTO>(cancellationToken: ct);
            return data is null
                ? Result<ReportResultDTO>.Fail("Failed to deserialize report data")
                : Result<ReportResultDTO>.Ok(data);
        }
        catch (Exception ex)
        {
            return Result<ReportResultDTO>.Fail($"Report generation unexpected error: {ex.Message}");
        }
    }

    public async Task<Result<ReportResultDTO>> GetByIdAsync(int id, CancellationToken ct)
    {
        try
        {
            var client = CreateConfiguredClient();
            var data = await client.GetFromJsonAsync<ReportResultDTO>($"/api/report/{id}", ct);
            return data is null
                ? Result<ReportResultDTO>.Fail("Report not found")
                : Result<ReportResultDTO>.Ok(data);
        }
        catch (Exception ex)
        {
            return Result<ReportResultDTO>.Fail($"Error retrieving report: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<ReportResultDTO>>> ListAsync(ReportFilterDTO filter, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var client = CreateConfiguredClient();
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var data = await client.GetFromJsonAsync<PagedResult<ReportResultDTO>>(
                $"/api/report?page={page}&pageSize={pageSize}", ct);
            return data is null
                ? Result<PagedResult<ReportResultDTO>>.Fail("No reports found")
                : Result<PagedResult<ReportResultDTO>>.Ok(data);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ReportResultDTO>>.Fail($"Error listing reports: {ex.Message}");
        }
    }

    public async Task<Result<ReportFileDTO>> ExportPdfAsync(int id, CancellationToken ct)
    {
        try
        {
            var client = CreateConfiguredClient();
            var bytes = await client.GetByteArrayAsync($"/api/report/{id}/export/pdf", ct);
            return Result<ReportFileDTO>.Ok(new ReportFileDTO
            {
                Content = bytes,
                FileName = $"report_{id}.pdf",
                ContentType = "application/pdf"
            });
        }
        catch (Exception ex)
        {
            return Result<ReportFileDTO>.Fail($"Error exporting PDF: {ex.Message}");
        }
    }

    public async Task<Result<ReportResultDTO>> ExportJsonAsync(int id, CancellationToken ct)
    {
        try
        {
            var client = CreateConfiguredClient();
            var data = await client.GetFromJsonAsync<ReportResultDTO>($"/api/report/{id}/export/json", ct);
            return data is null
                ? Result<ReportResultDTO>.Fail("Report not found")
                : Result<ReportResultDTO>.Ok(data);
        }
        catch (Exception ex)
        {
            return Result<ReportResultDTO>.Fail($"Error exporting JSON: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        try
        {
            var client = CreateConfiguredClient();
            var res = await client.DeleteAsync($"/api/report/{id}", ct);
            return res.IsSuccessStatusCode
                ? Result<bool>.Ok(true)
                : Result<bool>.Fail($"Failed to delete report: {res.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error deleting report: {ex.Message}");
        }
    }

    public Task<Result<bool>> RestoreAsync(int id, CancellationToken ct)
        => Task.FromResult(Result<bool>.Fail("Restore not supported for reports"));

    public async Task<Result<DashboardSummaryDTO>> GetDashboardAsync(CancellationToken ct)
    {
        try
        {
            var client = CreateConfiguredClient();
            var data = await client.GetFromJsonAsync<DashboardSummaryDTO>("/api/dashboard", ct);
            return data is null
                ? Result<DashboardSummaryDTO>.Fail("Dashboard not found")
                : Result<DashboardSummaryDTO>.Ok(data);
        }
        catch (Exception ex)
        {
            return Result<DashboardSummaryDTO>.Fail($"Error retrieving dashboard summary: {ex.Message}");
        }
    }

    private HttpClient CreateConfiguredClient()
    {
        var client = _http.CreateClient("ReportClient");
        var baseUrl = _cfg["DOC_URL"] ?? throw new InvalidOperationException("DOC_URL config key is missing.");
        var apiKey = _cfg["DOC_KEY"] ?? throw new InvalidOperationException("DOC_KEY security token is missing.");

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Remove("X-API-Key");
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        return client;
    }
}