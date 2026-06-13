using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Application.DTOs;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
[ResponseCache(Duration = 10, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _service.GetSummaryAsync(User, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("assets/stats")]
    public async Task<IActionResult> GetAssetStats(CancellationToken ct)
    {
        var result = await _service.GetAssetStatsAsync(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("vulns/trend")]
    public async Task<IActionResult> GetVulnTrend([FromQuery] TrendFilterDTO filter, CancellationToken ct)
    {
        var result = await _service.GetVulnTrendAsync(filter, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("risk/matrix")]
    public async Task<IActionResult> GetRiskMatrix(CancellationToken ct)
    {
        var result = await _service.GetRiskMatrixAsync(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("compliance")]
    public async Task<IActionResult> GetComplianceStatus(CancellationToken ct)
    {
        var result = await _service.GetComplianceStatusAsync(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] ActivityFilterDTO filter, CancellationToken ct)
    {
        var result = await _service.GetRecentActivityAsync(filter, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}