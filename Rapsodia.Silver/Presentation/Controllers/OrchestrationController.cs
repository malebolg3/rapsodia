// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Orleans;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class OrchestrationController : ControllerBase
{
    private readonly IGrainFactory _grains;
    private readonly IObsidianService _obsidian;

    public OrchestrationController(IGrainFactory grains, IObsidianService obsidian)
    {
        _grains = grains;
        _obsidian = obsidian;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetAgentsStatus()
    {
        var blue = _grains.GetGrain<IBlueAgent>(Guid.Empty);
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var silver = _grains.GetGrain<ISilverAgent>(Guid.Empty);

        var obsidianHealthy = await _obsidian.HealthCheckAsync();

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            obsidian = obsidianHealthy ? "healthy" : "unreachable",
            agents = new
            {
                blue = await blue.GetStatusAsync(),
                red = await red.GetStatusAsync(),
                violet = await violet.GetStatusAsync(),
                silver = await silver.GetStatusAsync()
            }
        });
    }

    [HttpPost("workflow/scan")]
    public async Task<IActionResult> StartScanWorkflow([FromBody] ScanWorkflowRequest request)
    {
        var blue = _grains.GetGrain<IBlueAgent>(Guid.Empty);
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);

        var labId = await violet.CreateLabAsync(request.Target, "kalilinux/kali-rolling:latest");
        var scanId = await red.StartScanAsync(request.Target, request.ScanType);
        await blue.TrackScanAsync(scanId, request.Target);

        return Ok(new { workflow = "scan", labId, scanId, status = "started" });
    }

    [HttpPost("workflow/pentest")]
    public async Task<IActionResult> StartPentestWorkflow([FromBody] PentestWorkflowRequest request)
    {
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var silver = _grains.GetGrain<ISilverAgent>(Guid.Empty);

        var labId = await violet.CreateLabAsync(request.Target, request.Image ?? "kalilinux/kali-rolling:latest");
        var exploitId = await red.StartExploitAsync(request.Target, request.ExploitName, request.Payload);
        var analysis = await silver.AnalyzeAsync($"Target: {request.Target}, Exploit: {request.ExploitName}");

        return Ok(new { workflow = "pentest", labId, exploitId, analysis = analysis[..Math.Min(200, analysis.Length)], status = "completed" });
    }

    [HttpPost("ai/analyze")]
    public async Task<IActionResult> AnalyzeWithAI([FromBody] AIAnalyzeRequest request)
    {
        var silver = _grains.GetGrain<ISilverAgent>(Guid.Empty);
        var input = $"Context: {request.Context}\nData: {request.Data}";
        var result = await silver.AnalyzeAsync(input);
        return Ok(new { analysis = result });
    }

    [HttpGet("events")]
    public async Task<IActionResult> SearchEvents([FromQuery] string query)
    {
        var results = await _obsidian.SearchNotesAsync("events", query);
        return Ok(new { count = results.Count, notes = results });
    }

    [HttpGet("knowledge/search")]
    public async Task<IActionResult> SearchKnowledge([FromQuery] string query)
    {
        var results = await _obsidian.SearchNotesAsync("ai-analysis", query);
        return Ok(new { count = results.Count, notes = results });
    }

    [HttpPost("agents/silver/context")]
    public async Task<IActionResult> SetSilverContext([FromBody] SetContextRequest request)
    {
        var silver = _grains.GetGrain<ISilverAgent>(Guid.Empty);
        await silver.SetContextAsync(request.Context);
        return Ok(new { status = "context updated" });
    }

    [HttpPost("agents/blue/scan")]
    public async Task<IActionResult> TriggerBlueScan([FromBody] ScanWorkflowRequest request)
    {
        var blue = _grains.GetGrain<IBlueAgent>(Guid.Empty);
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);

        var scanId = await red.StartScanAsync(request.Target, request.ScanType);
        await blue.TrackScanAsync(scanId, request.Target);

        return Ok(new { scanId, target = request.Target, status = "scan triggered" });
    }

    [HttpGet("agents/blue/assets")]
    public async Task<IActionResult> GetBlueAssets()
    {
        var blue = _grains.GetGrain<IBlueAgent>(Guid.Empty);
        var summary = await blue.GetAssetSummaryAsync();
        return Ok(new { assets = summary });
    }

    [HttpGet("agents/red/scans/{scanId}")]
    public async Task<IActionResult> GetRedScanResult(string scanId)
    {
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);
        var result = await red.GetScanResultAsync(scanId);
        return Ok(new { scanId, result });
    }

    [HttpPost("agents/red/exploit")]
    public async Task<IActionResult> TriggerRedExploit([FromBody] PentestWorkflowRequest request)
    {
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);

        var labId = await violet.CreateLabAsync(request.Target, request.Image ?? "kalilinux/kali-rolling:latest");
        var exploitId = await red.StartExploitAsync(request.Target, request.ExploitName, request.Payload);

        return Ok(new { exploitId, labId, target = request.Target, status = "exploit triggered" });
    }

    [HttpDelete("agents/violet/labs/{labId}")]
    public async Task<IActionResult> DestroyVioletLab(string labId)
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var result = await violet.DestroyLabAsync(labId);
        return result ? Ok(new { labId, status = "destroyed" }) : NotFound();
    }

    [HttpPost("workflow/full")]
    public async Task<IActionResult> StartFullWorkflow([FromBody] FullWorkflowRequest request)
    {
        var blue = _grains.GetGrain<IBlueAgent>(Guid.Empty);
        var red = _grains.GetGrain<IRedAgent>(Guid.Empty);
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var silver = _grains.GetGrain<ISilverAgent>(Guid.Empty);

        var labId = await violet.CreateLabAsync(request.Target, request.Image ?? "kalilinux/kali-rolling:latest");
        var scanId = await red.StartScanAsync(request.Target, "full");
        var exploitId = await red.StartExploitAsync(request.Target, request.ExploitName ?? "enumeration", null);
        await blue.TrackScanAsync(scanId, request.Target);
        await blue.NotifyIncidentAsync(Guid.NewGuid().ToString("N")[..8], "Full workflow executed", "high");

        var analysis = await silver.AnalyzeAsync($"Full pentest workflow on {request.Target}");

        return Ok(new
        {
            workflow = "full",
            labId,
            scanId,
            exploitId,
            analysis = analysis[..Math.Min(300, analysis.Length)],
            status = "completed"
        });
    }

    [HttpPost("agents/violet/sandbox")]
    public async Task<IActionResult> CreateSandbox([FromBody] SandboxRequest request)
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var labId = await violet.CreateSandboxAsync(request.Name, request.Image ?? "kalilinux/kali-rolling:latest", request.Tools, request.TtlMinutes);
        return Ok(new { labId, level = "sandbox", tools = request.Tools });
    }

    [HttpPost("agents/violet/honeypot")]
    public async Task<IActionResult> DeployHoneypot([FromBody] HoneypotRequest request)
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var labId = await violet.DeployHoneypotAsync(request.Name, request.HoneypotType, request.TtlMinutes);
        return Ok(new { labId, level = "honeypot", type = request.HoneypotType });
    }

    [HttpPost("agents/violet/soc")]
    public async Task<IActionResult> DeploySOC([FromBody] SOCRequest request)
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var labId = await violet.DeploySOCAsync(request.Name, request.TtlMinutes);
        return Ok(new { labId, level = "SOC", dashboard = "http://localhost:5601" });
    }

    [HttpPost("agents/violet/cyber-range")]
    public async Task<IActionResult> DeployCyberRange([FromBody] CyberRangeRequest request)
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var labId = await violet.DeployCyberRangeAsync(request.Name, request.Scenario, request.TtlMinutes);
        return Ok(new { labId, level = "cyber-range", scenario = request.Scenario });
    }

    [HttpPost("agents/violet/cleanup")]
    public async Task<IActionResult> CleanupExpired()
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var count = await violet.CleanupExpiredAsync();
        return Ok(new { cleaned = count, message = $"{count} labs expirados removidos" });
    }

    [HttpGet("agents/violet/labs")]
    public async Task<IActionResult> ListAllLabs()
    {
        var violet = _grains.GetGrain<IVioletAgent>(Guid.Empty);
        var labs = await violet.ListLabsAsync();
        return Ok(new { count = labs.Count, labs });
    }
}

public class ScanWorkflowRequest
{
    public string Target { get; set; } = string.Empty;
    public string ScanType { get; set; } = "full";
}

public class PentestWorkflowRequest
{
    public string Target { get; set; } = string.Empty;
    public string ExploitName { get; set; } = string.Empty;
    public string? Image { get; set; }
    public Dictionary<string, object>? Payload { get; set; }
}

public class AIAnalyzeRequest
{
    public string Context { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

public class SetContextRequest
{
    public string Context { get; set; } = string.Empty;
}

public class FullWorkflowRequest
{
    public string Target { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? ExploitName { get; set; }
}

public class SandboxRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public List<string> Tools { get; set; } = new() { "nmap", "gobuster", "nikto" };
    public int TtlMinutes { get; set; } = 240;
}

public class HoneypotRequest
{
    public string Name { get; set; } = string.Empty;
    public string HoneypotType { get; set; } = "ssh";
    public int TtlMinutes { get; set; } = 480;
}

public class SOCRequest
{
    public string Name { get; set; } = string.Empty;
    public int TtlMinutes { get; set; } = 480;
}

public class CyberRangeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Scenario { get; set; } = "ransomware";
    public int TtlMinutes { get; set; } = 1440;
}