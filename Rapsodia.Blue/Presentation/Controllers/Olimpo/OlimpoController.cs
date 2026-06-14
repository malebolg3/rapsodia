using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs.Olimpo;
using Rapsodia.Blue.Application.Interfaces.Olimpo;

namespace Rapsodia.Blue.Presentation.Controllers.Olimpo;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class OlimpoController : ControllerBase
{
    private readonly IOlimpoService _service;

    public OlimpoController(IOlimpoService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Login([FromForm] OlimpoLoginRequest request, CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpGet("ci/{key}")]
    [Authorize(Roles = "OLP_CI,OLP_ADMIN")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> GetCiSecret(string key, CancellationToken ct)
    {
        var result = await _service.GetCiSecretAsync(User, key, ct);
        if (!result.Success) return NotFound(result);
        return Ok(new { key, value = result.Data!.Value });
    }

    [HttpGet("ci/export/{environment}")]
    [Authorize(Roles = "OLP_CI,OLP_ADMIN")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ExportCiEnv(string environment, CancellationToken ct)
    {
        var result = await _service.ExportCiEnvAsync(User, environment, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("secrets")]
    public async Task<IActionResult> GetAllSecrets(CancellationToken ct)
    {
        var result = await _service.GetAllSecretsAsync(User, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("secrets/{id}")]
    public async Task<IActionResult> GetSecret(string id, CancellationToken ct)
    {
        var result = await _service.GetSecretByIdAsync(User, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("secrets")]
    public async Task<IActionResult> CreateSecret([FromBody] OlimpoCreateSecretRequest request, CancellationToken ct)
    {
        var result = await _service.CreateSecretAsync(User, request, ct);
        return result.Success ? CreatedAtAction(nameof(GetSecret), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("secrets/{id}")]
    public async Task<IActionResult> UpdateSecret(string id, [FromBody] OlimpoUpdateSecretRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateSecretAsync(User, id, request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("secrets/{id}")]
    public async Task<IActionResult> DeleteSecret(string id, CancellationToken ct)
    {
        var result = await _service.DeleteSecretAsync(User, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("totp")]
    public async Task<IActionResult> GetTotpAccounts(CancellationToken ct)
    {
        var result = await _service.GetAllSecretsAsync(User, ct);
        return Ok(result);
    }

    [HttpGet("totp/{id}")]
    public async Task<IActionResult> GetTotpCode(string id, CancellationToken ct)
    {
        var result = await _service.GenerateTotpCodeAsync(User, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("totp")]
    public async Task<IActionResult> AddTotpAccount([FromBody] OlimpoAddTotpRequest request, CancellationToken ct)
    {
        var result = await _service.AddTotpAccountAsync(User, request, ct);
        return result.Success ? CreatedAtAction(nameof(GetTotpCode), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpDelete("totp/{id}")]
    public async Task<IActionResult> DeleteTotpAccount(string id, CancellationToken ct)
    {
        var result = await _service.DeleteTotpAccountAsync(User, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments(CancellationToken ct)
    {
        var result = await _service.GetDocumentsAsync(User, ct);
        return Ok(result);
    }

    [HttpPost("documents")]
    public async Task<IActionResult> UploadDocument([FromForm] OlimpoUploadDocumentRequest request, CancellationToken ct)
    {
        var result = await _service.UploadDocumentAsync(User, request, ct);
        return result.Success ? CreatedAtAction(nameof(GetDocuments), null, result) : BadRequest(result);
    }

    [HttpDelete("documents/{id}")]
    public async Task<IActionResult> DeleteDocument(string id, CancellationToken ct)
    {
        var result = await _service.DeleteDocumentAsync(User, id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}