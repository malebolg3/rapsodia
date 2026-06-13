using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("Api")]
public class GraphController : ControllerBase
{
    private readonly IGraphService _service;
    private readonly ILogger<GraphController> _logger;

    public GraphController(IGraphService service, ILogger<GraphController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("topology")]
    [Authorize(Roles = "Admin,NetworkOperator,Analyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopology(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("User {User} requesting topology", User.Identity?.Name);
            var result = await _service.GetTopologyAsync(ct);
            
            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve topology: {Message}", result.Message);
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { Message = "Request cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving topology");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("path/{sourceId:int:min(1)}/{targetId:int:min(1)}")]
    [Authorize(Roles = "Admin,NetworkOperator,Analyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindPath(int sourceId, int targetId, CancellationToken ct)
    {
        try
        {
            if (sourceId == targetId)
            {
                return BadRequest(new { Success = false, Message = "Source and target nodes cannot be the same" });
            }

            _logger.LogInformation("Finding path from {SourceId} to {TargetId}", sourceId, targetId);
            var result = await _service.FindPathAsync(sourceId, targetId, ct);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding path from {SourceId} to {TargetId}", sourceId, targetId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPost("relation")]
    [Authorize(Roles = "Admin,NetworkOperator")]
    [EnableRateLimiting("graph-write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRelation([FromBody] CreateGraphRelationRequest request, CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Invalid request data", Errors = errors });
            }

            _logger.LogInformation("User {User} creating relation", User.Identity?.Name);
            var result = await _service.AddRelationAsync(request, ct);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetTopology), null, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating relation");
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPut("relation/{id:int:min(1)}")]
    [Authorize(Roles = "Admin,NetworkOperator")]
    [EnableRateLimiting("graph-write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRelation(int id, [FromBody] EditGraphRelationRequest request, CancellationToken ct)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Success = false, Message = "Invalid request data", Errors = errors });
            }

            _logger.LogInformation("User {User} updating relation {RelationId}", User.Identity?.Name, id);
            var result = await _service.UpdateRelationAsync(id, request, ct);
            
            if (!result.Success)
            {
                return result.Message?.Contains("not found") == true ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relation {RelationId}", id);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpDelete("relation/{id:int:min(1)}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("graph-write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRelation(int id, CancellationToken ct)
    {
        try
        {
            // Alerta de segurança: Remoção lógica auditada
            _logger.LogWarning("User {User} removing relation {RelationId}", User.Identity?.Name, id);
            var result = await _service.RemoveRelationAsync(id, ct);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing relation {RelationId}", id);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpPatch("relation/{id:int:min(1)}/restore")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("graph-write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreRelation(int id, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("User {User} restoring relation {RelationId}", User.Identity?.Name, id);
            var result = await _service.RestoreRelationAsync(id, ct);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring relation {RelationId}", id);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }

    [HttpGet("node/{nodeId:int:min(1)}/relations")]
    [Authorize(Roles = "Admin,NetworkOperator,Analyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNodeRelations(int nodeId, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching relations for node {NodeId}", nodeId);
            var result = await _service.GetNodeRelationsAsync(nodeId, ct);
            
            if (!result.Success)
            {
                return result.Message?.Contains("not found") == true ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching relations for node {NodeId}", nodeId);
            return StatusCode(500, new { Success = false, Message = "Internal server error" });
        }
    }
}