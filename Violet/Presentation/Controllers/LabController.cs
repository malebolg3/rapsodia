using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Violet.Application.DTOs;
using Rapsodia.Violet.Application.Interfaces;

namespace Rapsodia.Violet.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class LabController : ControllerBase
{
    private readonly ILabService _service;

    public LabController(ILabService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLab([FromBody] Rapsodia.Violet.Application.DTOs.CreateLabRequest request, CancellationToken ct)
    {
        var result = await _service.CreateLabAsync(request, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetLabStatus), new { containerId = result.Data!.ContainerId }, result);
    }

    [HttpPut("{containerId}")]
    public async Task<IActionResult> UpdateLab(string containerId, [FromBody] Rapsodia.Violet.Application.DTOs.CreateLabRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateLabAsync(containerId, request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{containerId}")]
    public async Task<IActionResult> GetLabStatus(string containerId, CancellationToken ct)
    {
        var result = await _service.GetLabStatusAsync(containerId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<IActionResult> ListLabs(CancellationToken ct)
    {
        var result = await _service.ListLabsAsync(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{containerId}/stop")]
    public async Task<IActionResult> StopLab(string containerId, CancellationToken ct)
    {
        var result = await _service.StopLabAsync(containerId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{containerId}/start")]
    public async Task<IActionResult> StartLab(string containerId, CancellationToken ct)
    {
        var result = await _service.StartLabAsync(containerId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{containerId}")]
    public async Task<IActionResult> RemoveLab(string containerId, CancellationToken ct)
    {
        var result = await _service.RemoveLabAsync(containerId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{containerId}/snapshots")]
    public async Task<IActionResult> CreateSnapshot(string containerId, [FromBody] CreateSnapshotRequest request, CancellationToken ct)
    {
        var result = await _service.CreateSnapshotAsync(containerId, request.SnapshotName, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetLabStatus), new { containerId }, result);
    }

    [HttpPost("{containerId}/snapshots/{snapshotId}/restore")]
    public async Task<IActionResult> RestoreSnapshot(string containerId, string snapshotId, CancellationToken ct)
    {
        var result = await _service.RestoreSnapshotAsync(containerId, snapshotId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{containerId}/exec")]
    public async Task<IActionResult> ExecuteCommand(string containerId, [FromBody] ExecCommandRequest request, CancellationToken ct)
    {
        var result = await _service.ExecuteCommandAsync(containerId, request.Command, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("provision")]
    public async Task<IActionResult> ProvisionLabs([FromBody] ProvisionLabRequest req, CancellationToken ct)
    {
        var containers = new List<ProvisionContainerInfo>();
        var ipCounter = 100;

        foreach (var lab in req.Labs)
        {
            var result = await _service.CreateLabAsync(new Rapsodia.Violet.Application.DTOs.CreateLabRequest
            {
                Name = lab.Name,
                Image = lab.Image,
                Network = lab.Network
            }, ct);

            if (result.Success)
            {
                containers.Add(new ProvisionContainerInfo
                {
                    Name = lab.Name,
                    Ip = $"10.10.20.{ipCounter++}",
                    Status = "running"
                });
            }
        }

        return Ok(new { success = true, containers });
    }
}

public class CreateSnapshotRequest
{
    public string SnapshotName { get; set; } = string.Empty;
}

public class ExecCommandRequest
{
    public string Command { get; set; } = string.Empty;
}

public class ProvisionLabRequest
{
    public List<ProvisionLabConfig> Labs { get; set; } = new();
    public string AdminUser { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}

public class ProvisionLabConfig
{
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Network { get; set; } = "isolated";
}

public class ProvisionContainerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}