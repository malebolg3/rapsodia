// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.DTOs.Auth;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Application.Services;

public class AuthAppService : IAuthService
{
    private readonly BlueDbContext _db;
    private readonly IConnectionMultiplexer _redis;

    private const string SESSION_KEY = "sessions";
    private const string BLACKLIST_PREFIX = "blacklist:";
    private const string FA2_PREFIX = "2fa:";

    public AuthAppService(BlueDbContext db, IConnectionMultiplexer redis)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    private IDatabase RedisDB => _redis.GetDatabase();

    public async Task<Result<AuthResultDTO>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.DeletedAt == null, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Result<AuthResultDTO>.Fail("Invalid credentials");

        return Result<AuthResultDTO>.Ok(GenerateToken(user, null, null, null));
    }

    public async Task<Result<AuthResultDTO>> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username, ct))
            return Result<AuthResultDTO>.Fail("Username already exists");

        var user = new User(req.Username, BCrypt.Net.BCrypt.HashPassword(req.Password), "Analyst", "blue");
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Result<AuthResultDTO>.Ok(GenerateToken(user, null, null, null));
    }

    public async Task<Result<AuthResultDTO>> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        if (await IsTokenBlacklistedAsync(req.Token))
            return Result<AuthResultDTO>.Fail("Token revoked");

        var principal = ValidateToken(req.Token);
        if (principal is null) return Result<AuthResultDTO>.Fail("Invalid token");

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null || user.DeletedAt != null) return Result<AuthResultDTO>.Fail("User not found");

        return Result<AuthResultDTO>.Ok(GenerateToken(user, null, null, null));
    }

    public async Task<Result<bool>> LogoutAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var token = user.FindFirst("jti")?.Value
                    ?? user.FindFirst("token")?.Value;

        if (string.IsNullOrEmpty(token))
            return Result<bool>.Ok(true);

        var expClaim = user.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (long.TryParse(expClaim, out var expUnix))
        {
            var expiry = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            var ttl = expiry - DateTime.UtcNow;
            if (ttl > TimeSpan.Zero)
                await RedisDB.StringSetAsync($"{BLACKLIST_PREFIX}{token}", "1", ttl);
        }

        return Result<bool>.Ok(true);
    }

    public async Task<Result<AuthResultDTO>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);

        return user is null || user.DeletedAt != null
            ? Result<AuthResultDTO>.Fail("User not found")
            : Result<AuthResultDTO>.Ok(new AuthResultDTO { UserId = user.Id, Username = user.Username, ExpiresAt = DateTime.UtcNow, AllowedModules = user.AllowedModules ?? string.Empty });
    }

    public async Task<Result<AuthResultDTO>> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null || user.DeletedAt != null) return Result<AuthResultDTO>.Fail("User not found");

        if (!string.IsNullOrEmpty(req.Email)) user.SetEmail(req.Email);
        if (!string.IsNullOrEmpty(req.FullName)) user.SetFullName(req.FullName);
        await _db.SaveChangesAsync(ct);

        return Result<AuthResultDTO>.Ok(new AuthResultDTO { UserId = user.Id, Username = user.Username, ExpiresAt = DateTime.UtcNow });
    }

    public async Task<Result<bool>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null || user.DeletedAt != null) return Result<bool>.Fail("User not found");

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash)) return Result<bool>.Fail("Current password is incorrect");

        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(req.NewPassword));
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<AuthorizeResponse>> RequestAuthorizationAsync(AuthorizeRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.DeletedAt == null, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Result<AuthorizeResponse>.Fail("Invalid credentials");

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var key = $"{FA2_PREFIX}{req.Username}";
        var value = $"{code}|{req.Service}|{req.Scope}|{req.ExpiresIn}";

        await RedisDB.StringSetAsync(key, value, TimeSpan.FromMinutes(5));
        Console.WriteLine($"2FA CODE for {req.Username}: {code}");

        return Result<AuthorizeResponse>.Ok(new AuthorizeResponse { Requires2FA = true, Message = "2FA code generated.", Username = req.Username });
    }

    public async Task<Result<SessionInfo>> Verify2FAAsync(Verify2FARequest req, CancellationToken ct)
    {
        var key = $"{FA2_PREFIX}{req.Username}";
        var stored = await RedisDB.StringGetAsync(key);

        if (!stored.HasValue)
            return Result<SessionInfo>.Fail("No pending authorization");

        var parts = stored.ToString().Split('|');
        if (parts.Length < 4)
        {
            await RedisDB.KeyDeleteAsync(key);
            return Result<SessionInfo>.Fail("Invalid 2FA data");
        }

        var code = parts[0];
        var service = parts[1];
        var scope = parts[2];
        var expiresIn = parts[3];

        if (code != req.Code)
            return Result<SessionInfo>.Fail("Invalid 2FA code");

        await RedisDB.KeyDeleteAsync(key);

        var user = await _db.Users.FirstAsync(u => u.Username == req.Username && u.DeletedAt == null, ct);
        var token = GenerateToken(user, service, scope, expiresIn);

        var session = new SessionInfo
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Username = req.Username,
            Service = service,
            Scope = scope,
            Token = token.Token,
            AuthorizedAt = DateTime.UtcNow,
            ExpiresAt = token.ExpiresAt,
            AuthorizedBy = "admin",
            IP = "localhost"
        };

        await RedisDB.HashSetAsync(SESSION_KEY, session.Id, JsonSerializer.Serialize(session));

        return Result<SessionInfo>.Ok(session);
    }

    public async Task<Result<List<SessionInfo>>> GetActiveSessionsAsync(CancellationToken ct)
    {
        var entries = await RedisDB.HashGetAllAsync(SESSION_KEY);
        var sessions = entries
            .Select(e => JsonSerializer.Deserialize<SessionInfo>(e.Value!))
            .Where(s => s != null && s.IsActive)
            .Select(s => s!)
            .ToList();

        return Result<List<SessionInfo>>.Ok(sessions);
    }

    public async Task<Result<bool>> RevokeSessionAsync(string sessionId, CancellationToken ct)
    {
        var data = await RedisDB.HashGetAsync(SESSION_KEY, sessionId);
        if (!data.HasValue)
            return Result<bool>.Fail("Session not found");

        var session = JsonSerializer.Deserialize<SessionInfo>(data!);
        if (session is null)
            return Result<bool>.Fail("Session not found");

        session.IsActive = false;
        await RedisDB.HashSetAsync(SESSION_KEY, sessionId, JsonSerializer.Serialize(session));

        return Result<bool>.Ok(true);
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return RedisDB.KeyExistsAsync($"{BLACKLIST_PREFIX}{token}");
    }

    private static AuthResultDTO GenerateToken(User user, string? service, string? scope, string? expiresIn)
    {
        var authKey = Environment.GetEnvironmentVariable("AUTH_KEY") ?? throw new InvalidOperationException("AUTH_KEY nula");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey));

        var hours = expiresIn switch { "1h" => 1, "24h" => 24, _ => 8 };
        var expires = DateTime.UtcNow.AddHours(hours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("allowed_modules", user.AllowedModules ?? string.Empty)
        };

        if (!string.IsNullOrEmpty(service) && !string.IsNullOrEmpty(scope))
        {
            claims.Add(new("scope", $"{service}:{scope}"));
            claims.Add(new("service", service));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = Environment.GetEnvironmentVariable("AUTH_ISS"),
            Audience = Environment.GetEnvironmentVariable("AUTH_AUD"),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler();

        return new AuthResultDTO
        {
            UserId = user.Id,
            Username = user.Username,
            Token = handler.WriteToken(handler.CreateToken(tokenDescriptor)),
            RefreshToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = expires,
            AllowedModules = user.AllowedModules ?? string.Empty
        };
    }

    private static ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var authKey = Environment.GetEnvironmentVariable("AUTH_KEY") ?? throw new InvalidOperationException("AUTH_KEY nula");
            return new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
                ValidateIssuer = true,
                ValidIssuer = Environment.GetEnvironmentVariable("AUTH_ISS"),
                ValidateAudience = true,
                ValidAudience = Environment.GetEnvironmentVariable("AUTH_AUD"),
                ValidateLifetime = true
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}