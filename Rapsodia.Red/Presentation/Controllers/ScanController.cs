using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Red.Application.DTOs;
using Rapsodia.Red.Application.Interfaces;

namespace Rapsodia.Red.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class ScanController : ControllerBase
{
    private readonly IScanService _service;

    public ScanController(IScanService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> StartScan([FromBody] ScanRequest request, CancellationToken ct)
    {
        var result = await _service.StartScanAsync(request, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetStatus), new { scanId = result.Data!.ScanId }, result);
    }

    [HttpGet("{scanId:guid}")]
    public async Task<IActionResult> GetStatus(Guid scanId, CancellationToken ct)
    {
        var result = await _service.GetScanStatusAsync(scanId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]

    public async Task<IActionResult> ListScans([FromQuery] ScanFilterDTO filter, CancellationToken ct)
    {
        var result = await _service.ListScansAsync(filter, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{scanId:guid}/stop")]
    public async Task<IActionResult> StopScan(Guid scanId, CancellationToken ct)
    {
        var result = await _service.StopScanAsync(scanId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("{scanId:guid}/findings")]
    public async Task<IActionResult> GetFindings(Guid scanId, CancellationToken ct)
    {
        var result = await _service.GetFindingsAsync(scanId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}