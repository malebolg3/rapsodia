// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    private readonly Dictionary<string, (string Code, DateTime Expires, AuthorizeRequest Request)> _pending2FA = new();
    private readonly List<SessionInfo> _activeSessions = new();
    private readonly Random _random = new();

    public AuthAppService(BlueDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Result<AuthResultDTO>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.DeletedAt == null, ct);
        if (user is null || !VerifyPassword(req.Password, user.PasswordHash))
            return Result<AuthResultDTO>.Fail("Invalid credentials");

        var token = GenerateToken(user);
        return Result<AuthResultDTO>.Ok(token);
    }

    public async Task<Result<AuthResultDTO>> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username, ct))
            return Result<AuthResultDTO>.Fail("Username already exists");

        var user = new User(req.Username, HashPassword(req.Password), "Analyst", "blue");
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var token = GenerateToken(user);
        return Result<AuthResultDTO>.Ok(token);
    }

    public async Task<Result<AuthResultDTO>> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var principal = ValidateToken(req.Token);
        if (principal is null)
            return Result<AuthResultDTO>.Fail("Invalid token");

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null || user.DeletedAt != null)
            return Result<AuthResultDTO>.Fail("User not found");

        var token = GenerateToken(user);
        return Result<AuthResultDTO>.Ok(token);
    }

    public Task<Result<bool>> LogoutAsync(ClaimsPrincipal user, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public async Task<Result<AuthResultDTO>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        
        return user is null || user.DeletedAt != null
            ? Result<AuthResultDTO>.Fail("User not found") 
            : Result<AuthResultDTO>.Ok(new AuthResultDTO
            {
                UserId = user.Id,
                Username = user.Username,
                Token = "",
                RefreshToken = "",
                ExpiresAt = DateTime.UtcNow,
                AllowedModules = user.AllowedModules
            });
    }

    public async Task<Result<AuthResultDTO>> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null || user.DeletedAt != null) return Result<AuthResultDTO>.Fail("User not found");
        
        if (!string.IsNullOrEmpty(req.Email)) user.SetEmail(req.Email);
        if (!string.IsNullOrEmpty(req.FullName)) user.SetFullName(req.FullName);
        await _db.SaveChangesAsync(ct);
        
        return Result<AuthResultDTO>.Ok(new AuthResultDTO
        {
            UserId = user.Id,
            Username = user.Username,
            Token = "",
            RefreshToken = "",
            ExpiresAt = DateTime.UtcNow
        });
    }

    public async Task<Result<bool>> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null || user.DeletedAt != null) return Result<bool>.Fail("User not found");

        if (!VerifyPassword(req.CurrentPassword, user.PasswordHash))
            return Result<bool>.Fail("Current password is incorrect");

        user.SetPasswordHash(HashPassword(req.NewPassword));
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<AuthorizeResponse>> RequestAuthorizationAsync(AuthorizeRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.DeletedAt == null, ct);
        if (user is null || !VerifyPassword(req.Password, user.PasswordHash))
            return Result<AuthorizeResponse>.Fail("Invalid credentials");

        var code = _random.Next(100000, 999999).ToString();
        _pending2FA[req.Username] = (code, DateTime.UtcNow.AddMinutes(5), req);
        Console.WriteLine($"2FA CODE for {req.Username}: {code}");
        
        _ = Task.Run(() => SendEmailCode(user.Username + "@rapsodia.local", code), ct);
        
        return Result<AuthorizeResponse>.Ok(new AuthorizeResponse
        {
            Requires2FA = true,
            Message = "2FA code sent to email and console.",
            Username = req.Username
        });
    }

    public Task<Result<SessionInfo>> Verify2FAAsync(Verify2FARequest req, CancellationToken ct)
    {
        if (!_pending2FA.TryGetValue(req.Username, out var pending))
            return Task.FromResult(Result<SessionInfo>.Fail("No pending authorization"));
        
        if (pending.Expires < DateTime.UtcNow)
        {
            _pending2FA.Remove(req.Username);
            return Task.FromResult(Result<SessionInfo>.Fail("2FA code expired"));
        }
        
        if (pending.Code != req.Code)
            return Task.FromResult(Result<SessionInfo>.Fail("Invalid 2FA code"));
        
        _pending2FA.Remove(req.Username);
        
        var user = _db.Users.First(u => u.Username == req.Username && u.DeletedAt == null);
        var token = GenerateScopedToken(user, pending.Request.Service, pending.Request.Scope, pending.Request.ExpiresIn);
        
        var session = new SessionInfo
        {
            Username = req.Username,
            Service = pending.Request.Service,
            Scope = pending.Request.Scope,
            Token = token.Token,
            AuthorizedAt = DateTime.UtcNow,
            ExpiresAt = token.ExpiresAt,
            AuthorizedBy = "admin",
            IP = "localhost"
        };
        
        _activeSessions.Add(session);
        
        return Task.FromResult(Result<SessionInfo>.Ok(session));
    }

    public Task<Result<List<SessionInfo>>> GetActiveSessionsAsync(CancellationToken ct)
    {
        var sessions = _activeSessions.Where(s => s.IsActive).ToList();
        return Task.FromResult(Result<List<SessionInfo>>.Ok(sessions));
    }

    public Task<Result<bool>> RevokeSessionAsync(string sessionId, CancellationToken ct)
    {
        var session = _activeSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session is null)
            return Task.FromResult(Result<bool>.Fail("Session not found"));
        
        session.IsActive = false;
        return Task.FromResult(Result<bool>.Ok(true));
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private AuthResultDTO GenerateToken(User user)
    {
        var (authKey, authIss, authAud) = GetAuthConfig();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("allowed_modules", user.AllowedModules)
        };

        var expires = DateTime.UtcNow.AddHours(8);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = authIss,
            Audience = authAud,
            SigningCredentials = creds
        };
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return new AuthResultDTO
        {
            UserId = user.Id,
            Username = user.Username,
            Token = handler.WriteToken(token),
            RefreshToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = expires,
            AllowedModules = user.AllowedModules
        };
    }

    private AuthResultDTO GenerateScopedToken(User user, string service, string scope, string expiresIn)
    {
        var (authKey, authIss, authAud) = GetAuthConfig();
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var hours = expiresIn switch
        {
            "1h" => 1,
            "24h" => 24,
            _ => 8
        };
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("scope", $"{service}:{scope}"),
            new("service", service),
            new("authorized_by", "admin"),
            new(ClaimTypes.Role, user.Role),
            new("allowed_modules", user.AllowedModules)
        };
        
        var expires = DateTime.UtcNow.AddHours(hours);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = authIss,
            Audience = authAud,
            SigningCredentials = creds
        };
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        
        return new AuthResultDTO
        {
            UserId = user.Id,
            Username = user.Username,
            Token = handler.WriteToken(token),
            RefreshToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = expires,
            AllowedModules = user.AllowedModules
        };
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var (authKey, authIss, authAud) = GetAuthConfig();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey));
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = authIss,
                ValidateAudience = true,
                ValidAudience = authAud,
                ValidateLifetime = true
            }, out _);
        }
        catch
        {
            return null;
        }
    }

    private static (string key, string issuer, string audience) GetAuthConfig()
    {
        var authKey = Environment.GetEnvironmentVariable("AUTH_KEY") 
            ?? throw new InvalidOperationException("AUTH_KEY nao configurada");
        
        var authIss = Environment.GetEnvironmentVariable("AUTH_ISS") 
            ?? throw new InvalidOperationException("AUTH_ISS nao configurada");
        
        var authAud = Environment.GetEnvironmentVariable("AUTH_AUD") 
            ?? throw new InvalidOperationException("AUTH_AUD nao configurada");
        
        return (authKey, authIss, authAud);
    }

    private async Task SendEmailCode(string email, string code)
    {
        try
        {
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
            
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                Console.WriteLine($"[EMAIL] SMTP not configured. Code for {email}: {code}");
                return;
            }
            
            using var smtp = new SmtpClient(smtpHost, smtpPort);
            smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
            smtp.EnableSsl = true;
            
            using var message = new MailMessage(smtpUser, email, "Rapsodia 2FA - Codigo de Acesso", 
                $"Seu codigo de verificacao: {code}\n\nValido por 5 minutos.");
            
            await smtp.SendMailAsync(message);
            Console.WriteLine($"[EMAIL] 2FA code sent to {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL] Failed: {ex.Message}. Code for {email}: {code}");
        }
    }
}