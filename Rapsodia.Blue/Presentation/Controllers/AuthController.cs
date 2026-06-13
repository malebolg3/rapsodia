using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.DTOs.Auth;
using Rapsodia.Blue.Application.Interfaces;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost("login")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _service.LoginAsync(request, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("register")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(request, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(Login), null, result);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _service.RefreshTokenAsync(request, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var result = await _service.LogoutAsync(User, ct);
        return result.Success ? NoContent() : BadRequest(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await _service.GetCurrentUserAsync(User, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateProfileAsync(User, request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _service.ChangePasswordAsync(User, request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("authorize")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Authorize([FromBody] AuthorizeRequest request, CancellationToken ct)
    {
        var result = await _service.RequestAuthorizationAsync(request, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("verify-2fa")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request, CancellationToken ct)
    {
        var result = await _service.Verify2FAAsync(request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions(CancellationToken ct)
    {
        var result = await _service.GetActiveSessionsAsync(ct);
        return Ok(result);
    }

    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(string sessionId, CancellationToken ct)
    {
        var result = await _service.RevokeSessionAsync(sessionId, ct);
        return result.Success ? NoContent() : NotFound(result);
    }
}