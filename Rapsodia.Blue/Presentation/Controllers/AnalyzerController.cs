using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class AnalyzerController : ControllerBase
{
    private readonly ISecurityAnalyzer _service;

    public AnalyzerController(ISecurityAnalyzer service)
    {
        _service = service;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] CreateAnalyzeRequest request, CancellationToken ct)
    {
        var result = await _service.AnalyzeAsync(request, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetReport), new { id = result.Data?.Id }, result);
    }

    [HttpGet("report/{id:int}")]
    public async Task<IActionResult> GetReport(int id, CancellationToken ct)
    {
        var result = await _service.GetReportAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("reports")]
    public async Task<IActionResult> ListReports([FromQuery] AnalyzerFilterDTO filter, CancellationToken ct)
    {
        var result = await _service.ListReportsAsync(filter, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> BatchAnalyze([FromBody] BatchAnalyzeRequest request, CancellationToken ct)
    {
        var result = await _service.BatchAnalyzeAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("report/{id:int}")]
    public async Task<IActionResult> DeleteReport(int id, CancellationToken ct)
    {
        var result = await _service.DeleteReportAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("report/{id:int}/restore")]
    public async Task<IActionResult> RestoreReport(int id, CancellationToken ct)
    {
        var result = await _service.RestoreReportAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _service.GetStatsAsync(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}