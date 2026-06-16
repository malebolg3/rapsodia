using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class IncidentService : IIncidentService
{
    private readonly List<IncidentResultDTO> _incidents = new();
    private readonly IConfiguration _cfg;
    private readonly bool _mock;
    private int _nextId = 1;

    public IncidentService(IConfiguration cfg)
    {
        _cfg = cfg;
        _mock = cfg["INC_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DB_HOST"]);
        
        if (_mock)
        {
            _incidents.Add(new IncidentResultDTO
            {
                Id = _nextId++,
                Title = "Ransomware Attack Detected",
                Description = "Encryption activity detected on file server",
                Severity = "Critical",
                Status = "Open",
                AssetId = 10,
                AssignedToId = 1,
                AssignedToName = "Admin User",
                Comments = new List<IncidentCommentDTO>
                {
                    new() { Id = 1, Content = "Investigating the source", AuthorName = "Admin User", CreatedAt = DateTime.UtcNow.AddHours(-4) },
                    new() { Id = 2, Content = "Containment in progress", AuthorName = "Analyst", CreatedAt = DateTime.UtcNow.AddHours(-2) }
                },
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            });
            _incidents.Add(new IncidentResultDTO
            {
                Id = _nextId++,
                Title = "Unauthorized Access Attempt",
                Description = "Multiple failed login attempts from external IP",
                Severity = "High",
                Status = "InProgress",
                AssetId = 5,
                AssignedToId = 2,
                AssignedToName = "Security Analyst",
                Comments = new List<IncidentCommentDTO>
                {
                    new() { Id = 3, Content = "IP blocked at firewall", AuthorName = "Security Analyst", CreatedAt = DateTime.UtcNow.AddHours(-1) }
                },
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            });
            _incidents.Add(new IncidentResultDTO
            {
                Id = _nextId++,
                Title = "Data Exfiltration Alert",
                Description = "Large outbound data transfer detected",
                Severity = "Medium",
                Status = "Resolved",
                AssetId = 15,
                AssignedToId = 1,
                AssignedToName = "Admin User",
                Comments = new List<IncidentCommentDTO>(),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ResolvedAt = DateTime.UtcNow.AddDays(-1)
            });
        }
    }

    public Task<Result<IncidentResultDTO>> CreateAsync(ClaimsPrincipal user, CreateIncidentRequest req, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var userName = GetUserName(user);
        
        var incident = new IncidentResultDTO
        {
            Id = _nextId++,
            Title = req.Title,
            Description = req.Description,
            Severity = req.Severity,
            Status = "Open",
            AssetId = req.AssetId,
            AssignedToId = userId,
            AssignedToName = userName,
            Comments = new List<IncidentCommentDTO>(),
            CreatedAt = DateTime.UtcNow
        };
        
        _incidents.Add(incident);
        return Task.FromResult(Result<IncidentResultDTO>.Ok(incident));
    }

    public Task<Result<IncidentResultDTO>> GetByIdAsync(int id, CancellationToken ct)
    {
        var incident = _incidents.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(incident is null 
            ? Result<IncidentResultDTO>.Fail("Incident not found") 
            : Result<IncidentResultDTO>.Ok(incident));
    }

    public Task<Result<PagedResult<IncidentResultDTO>>> ListAsync(IncidentFilterDTO filter, CancellationToken ct)
    {
        var items = _incidents
            .Where(i => string.IsNullOrEmpty(filter.Severity) || i.Severity == filter.Severity)
            .Where(i => string.IsNullOrEmpty(filter.Status) || i.Status == filter.Status)
            .Where(i => !filter.AssetId.HasValue || i.AssetId == filter.AssetId.Value)
            .ToList();

        return Task.FromResult(Result<PagedResult<IncidentResultDTO>>.Ok(new PagedResult<IncidentResultDTO>
        {
            Items = items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            TotalCount = items.Count,
            Page = filter.Page,
            PageSize = filter.PageSize
        }));
    }

    public Task<Result<IncidentResultDTO>> UpdateAsync(int id, EditIncidentRequest req, CancellationToken ct)
    {
        var incident = _incidents.FirstOrDefault(i => i.Id == id);
        if (incident is null)
            return Task.FromResult(Result<IncidentResultDTO>.Fail("Incident not found"));

        if (req.Title != null) incident.Title = req.Title;
        if (req.Description != null) incident.Description = req.Description;
        if (req.Severity != null) incident.Severity = req.Severity;

        return Task.FromResult(Result<IncidentResultDTO>.Ok(incident));
    }

    public Task<Result<IncidentResultDTO>> AssignAsync(int id, AssignIncidentRequest req, CancellationToken ct)
    {
        var incident = _incidents.FirstOrDefault(i => i.Id == id);
        if (incident is null)
            return Task.FromResult(Result<IncidentResultDTO>.Fail("Incident not found"));

        incident.AssignedToId = req.UserId;
        incident.AssignedToName = $"User_{req.UserId}";
        return Task.FromResult(Result<IncidentResultDTO>.Ok(incident));
    }

    public Task<Result<IncidentResultDTO>> ChangeStatusAsync(int id, ChangeStatusRequest req, CancellationToken ct)
    {
        var incident = _incidents.FirstOrDefault(i => i.Id == id);
        if (incident is null)
            return Task.FromResult(Result<IncidentResultDTO>.Fail("Incident not found"));

        incident.Status = req.Status;
        if (req.Status == "Resolved")
            incident.ResolvedAt = DateTime.UtcNow;

        return Task.FromResult(Result<IncidentResultDTO>.Ok(incident));
    }

    public Task<Result<IncidentCommentDTO>> AddCommentAsync(int id, ClaimsPrincipal user, AddCommentRequest req, CancellationToken ct)
    {
        var incident = _incidents.FirstOrDefault(i => i.Id == id);
        if (incident is null)
            return Task.FromResult(Result<IncidentCommentDTO>.Fail("Incident not found"));

        var comment = new IncidentCommentDTO
        {
            Id = incident.Comments.Count + 1,
            Content = req.Content,
            AuthorName = GetUserName(user),
            CreatedAt = DateTime.UtcNow
        };
        incident.Comments.Add(comment);
        return Task.FromResult(Result<IncidentCommentDTO>.Ok(comment));
    }

    public Task<Result<bool>> CloseAsync(int id, ClaimsPrincipal user, CancellationToken ct)
    {
        var incident = _incidents.FirstOrDefault(i => i.Id == id);
        if (incident is null)
            return Task.FromResult(Result<bool>.Fail("Incident not found"));

        incident.Status = "Closed";
        incident.ResolvedAt = DateTime.UtcNow;
        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<IncidentStatsDTO>> GetStatsAsync(CancellationToken ct)
    {
        return Task.FromResult(Result<IncidentStatsDTO>.Ok(new IncidentStatsDTO
        {
            TotalIncidents = _incidents.Count,
            OpenIncidents = _incidents.Count(i => i.Status == "Open"),
            InProgressIncidents = _incidents.Count(i => i.Status == "InProgress"),
            ResolvedIncidents = _incidents.Count(i => i.Status == "Resolved" || i.Status == "Closed"),
            BySeverity = _incidents.GroupBy(i => i.Severity).ToDictionary(g => g.Key, g => g.Count())
        }));
    }

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    private static string GetUserName(ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
}