using Rapsodia.DTO.Response;
using Rapsodia.DTO.TelemetryDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using rapsodia.Services.Telemetry;
using Rapsodia.Services.Telemetries;

namespace Rapsodia.Controllers
{
    [Route("api/telemetry")]
    [ApiController]
    [RequireRateLimiting("auth-limit")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(ITelemetryService telemetryService, ILogger<TelemetryController> logger)
        {
            _telemetryService = telemetryService;
            _logger = logger;
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseModel<TelemetryStatsDTO>>> Stats()
        {
            var response = await _telemetryService.GetStats();
            return response.Status ? Ok(response) : BadRequest(response);
        }

        [HttpGet("stream")]
        [Authorize(Roles = "Admin")]
        public async Task Stream(CancellationToken ct)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            while (!ct.IsCancellationRequested)
            {
                var response = await _telemetryService.GetLatestTelemetries(1);

                if (response.Status && response.Dados?.Count > 0)
                {
                    var data = response.Dados.First();
                    var json = JsonSerializer.Serialize(new
                    {
                        valor = data.EntropyValue,
                        timestamp = data.AnalysisTimestamp.ToString("HH:mm:ss")
                    });

                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync();
                }

                await Task.Delay(2000, ct);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseModel<TelemetryResponseDTO>>> GetById([FromRoute] int id)
        {
            var response = await _telemetryService.GetTelemetryById(id);
            return response.Status ? Ok(response) : NotFound(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseModel<TelemetryResponseDTO>>> Create(
            [FromBody] TelemetryCreateDTO dto,
            [FromHeader(Name = "X-API-KEY")] string? apiKey)
        {
            var response = await _telemetryService.CreateTelemetry(dto, apiKey);
            if (!response.Status) return BadRequest(response);
            return CreatedAtAction(nameof(GetById), new { id = response.Dados?.Id }, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseModel<bool>>> Delete([FromRoute] int id)
        {
            var response = await _telemetryService.DeleteTelemetry(id);
            return response.Status ? Ok(response) : NotFound(response);
        }

        [HttpPost("analyze")]
        [AllowAnonymous]
        [RequireRateLimiting("auth-limit")]
        public async Task<IActionResult> Analyze(
            [FromBody] LogAnalysisRequest req,
            [FromServices] GeminiService gemini,
            [FromServices] ObsidianService obsidian)
        {
            var result = await gemini.AnalyzeSecurityThreat(req.LogContent, req.Environment);
            await obsidian.SaveNote(result, req.Environment);
            return Ok(new { analysis = result, saved = true });
        }
    }

    public class LogAnalysisRequest
    {
        public string LogContent { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }
}