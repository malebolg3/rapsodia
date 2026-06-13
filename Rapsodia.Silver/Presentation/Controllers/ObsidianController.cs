using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Silver.Application.Interfaces;

namespace Rapsodia.Silver.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class ObsidianController : ControllerBase
{
    private readonly IObsidianService _obsidian;

    public ObsidianController(IObsidianService obsidian)
    {
        _obsidian = obsidian;
    }

    [HttpPost("vaults/{vault}/notes")]
    public async Task<IActionResult> AppendNote(string vault, [FromBody] AppendNoteRequest request)
    {
        var noteId = await _obsidian.AppendNoteAsync(vault, request.Content);
        return Ok(new { noteId, vault, status = "appended" });
    }

    [HttpGet("vaults/{vault}/notes/{noteId}")]
    public async Task<IActionResult> ReadNote(string vault, string noteId)
    {
        var content = await _obsidian.ReadNoteAsync(vault, noteId);
        return string.IsNullOrEmpty(content) ? NotFound() : Ok(new { noteId, vault, content });
    }

    [HttpGet("vaults/{vault}/search")]
    public async Task<IActionResult> SearchNotes(string vault, [FromQuery] string query)
    {
        var results = await _obsidian.SearchNotesAsync(vault, query);
        return Ok(new { vault, query, count = results.Count, notes = results });
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        var healthy = await _obsidian.HealthCheckAsync();
        return Ok(new
        {
            service = "Obsidian",
            status = healthy ? "healthy" : "unreachable",
            timestamp = DateTime.UtcNow
        });
    }
}

public class AppendNoteRequest
{
    public string Content { get; set; } = string.Empty;
}