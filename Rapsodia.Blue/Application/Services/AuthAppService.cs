using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.DTOs.Auth;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class AuthAppService : IAuthService
{
    private readonly List<UserResultDTO> _users = new();
    private readonly Dictionary<string, (string Code, DateTime Expires, AuthorizeRequest Request)> _pending2FA = new();
    private readonly List<SessionInfo> _activeSessions = new();
    private readonly Random _random = new();
    private int _nextId = 1;

    public AuthAppService()
    {
        _users.Add(new UserResultDTO
        {
            Id = _nextId++,
            Username = "admin",
            Email = "admin@rapsodia.local",
            FullName = "Admin User",
            IsActive = true,
            Roles = new List<string> { "Admin" },
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        });
    }

    public Task<Result<AuthResultDTO>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = _users.FirstOrDefault(u => u.Username == req.Username && u.IsActive);
        if (user is null || req.Password != "admin123")
            return Task.FromResult(Result<AuthResultDTO>.Fail("Invalid credentials"));

        var token = GenerateToken(user);
        return Task.FromResult(Result<AuthResultDTO>.Ok(token));
    }

    public Task<Result<AuthResultDTO>> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var user = new UserResultDTO
        {
            Id = _nextId++,
            Username = req.Username,
            Email = req.Email,
            FullName = req.FullName,
            IsActive = true,
            Roles = new List<string> { "User" },
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);

        var token = GenerateToken(user);
        return Task.FromResult(Result<AuthResultDTO>.Ok(token));
    }

    public Task<Result<AuthResultDTO>> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var principal = ValidateToken(req.Token);
        if (principal is null)
            return Task.FromResult(Result<AuthResultDTO>.Fail("Invalid token"));

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user is null)
            return Task.FromResult(Result<AuthResultDTO>.Fail("User not found"));

        var token = GenerateToken(user);
        return Task.FromResult(Result<AuthResultDTO>.Ok(token));
    }

    public Task<Result<bool>> LogoutAsync(ClaimsPrincipal user, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public Task<Result<AuthResultDTO>> GetCurrentUserAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var username = user.FindFirst(ClaimTypes.Name)!.Value;
        var usr = _users.FirstOrDefault(u => u.Id == userId);
        
        return Task.FromResult(usr is null 
            ? Result<AuthResultDTO>.Fail("User not found") 
            : Result<AuthResultDTO>.Ok(new AuthResultDTO
            {
                UserId = usr.Id,
                Username = usr.Username,
                Token = "",
                RefreshToken = "",
                ExpiresAt = DateTime.UtcNow
            }));
    }

    public Task<Result<AuthResultDTO>> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var usr = _users.FirstOrDefault(u => u.Id == userId);
        if (usr is null) return Task.FromResult(Result<AuthResultDTO>.Fail("User not found"));
        
        if (req.Email != null) usr.Email = req.Email;
        if (req.FullName != null) usr.FullName = req.FullName;
        
        return Task.FromResult(Result<AuthResultDTO>.Ok(new AuthResultDTO
        {
            UserId = usr.Id,
            Username = usr.Username,
            Token = "",
            RefreshToken = "",
            ExpiresAt = DateTime.UtcNow
        }));
    }

    public Task<Result<bool>> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequest req, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public Task<Result<AuthorizeResponse>> RequestAuthorizationAsync(AuthorizeRequest req, CancellationToken ct)
    {
        var user = _users.FirstOrDefault(u => u.Username == req.Username && u.IsActive);
        if (user is null || req.Password != "admin123")
            return Task.FromResult(Result<AuthorizeResponse>.Fail("Invalid credentials"));

        var code = _random.Next(100000, 999999).ToString();
        _pending2FA[req.Username] = (code, DateTime.UtcNow.AddMinutes(5), req);
        Console.WriteLine($"2FA CODE for {req.Username}: {code}");
        
        _ = Task.Run(() => SendEmailCode(user.Email, code));
        
        return Task.FromResult(Result<AuthorizeResponse>.Ok(new AuthorizeResponse
        {
            Requires2FA = true,
            Message = "2FA code sent to email and console.",
            Username = req.Username
        }));
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
        
        var user = _users.First(u => u.Username == req.Username);
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

    private AuthResultDTO GenerateToken(UserResultDTO user)
    {
        var (authKey, authIss, authAud) = GetAuthConfig();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

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
            ExpiresAt = expires
        };
    }

    private AuthResultDTO GenerateScopedToken(UserResultDTO user, string service, string scope, string expiresIn)
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
            new("authorized_by", "admin")
        };
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        
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
            ExpiresAt = expires
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