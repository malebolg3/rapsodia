// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;
using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Services;

public class IncidentService : IIncidentService
{
    private readonly IIncidentRepository _repository;

    public IncidentService(IIncidentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Result<IncidentResultDTO>> CreateAsync(ClaimsPrincipal user, CreateIncidentRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        try
        {
            var entity = new Incident();
            entity.Update(
                title: req.Title,
                description: req.Description,
                severity: req.Severity,
                status: "Open",
                assetId: req.AssetId
            );

            var userName = GetUserName(user);
            if (!string.IsNullOrEmpty(userName))
                entity.Assign(null, userName);

            await _repository.SaveAsync(entity, ct);
            return Result<IncidentResultDTO>.Ok(MapToResult(entity));
        }
        catch (Exception ex)
        {
            return Result<IncidentResultDTO>.Fail($"Error creating incident: {ex.Message}");
        }
    }

    public async Task<Result<IncidentResultDTO>> GetByIdAsync(int id, CancellationToken ct)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            return entity is null
                ? Result<IncidentResultDTO>.Fail("Incident not found")
                : Result<IncidentResultDTO>.Ok(MapToResult(entity));
        }
        catch (Exception ex)
        {
            return Result<IncidentResultDTO>.Fail($"Error retrieving incident: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<IncidentResultDTO>>> ListAsync(IncidentFilterDTO filter, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var entities = await _repository.ListAllAsync(ct);

            var items = entities
                .Where(i => string.IsNullOrEmpty(filter.Severity) || i.Severity == filter.Severity)
                .Where(i => string.IsNullOrEmpty(filter.Status) || i.Status == filter.Status)
                .Where(i => !filter.AssetId.HasValue || i.AssetId == filter.AssetId.Value)
                .ToList();

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            return Result<PagedResult<IncidentResultDTO>>.Ok(new PagedResult<IncidentResultDTO>
            {
                Items = items.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResult).ToList(),
                TotalCount = items.Count,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return Result<PagedResult<IncidentResultDTO>>.Fail($"Error listing incidents: {ex.Message}");
        }
    }

    public async Task<Result<IncidentResultDTO>> UpdateAsync(int id, EditIncidentRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        try
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity is null)
                return Result<IncidentResultDTO>.Fail("Incident not found");

            entity.Update(
                title: req.Title,
                description: req.Description,
                severity: req.Severity
            );

            await _repository.UpdateAsync(entity, ct);
            return Result<IncidentResultDTO>.Ok(MapToResult(entity));
        }
        catch (Exception ex)
        {
            return Result<IncidentResultDTO>.Fail($"Error updating incident: {ex.Message}");
        }
    }

    public async Task<Result<IncidentResultDTO>> AssignAsync(int id, AssignIncidentRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        try
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity is null)
                return Result<IncidentResultDTO>.Fail("Incident not found");

            entity.Assign(req.UserId, $"User_{req.UserId}");

            await _repository.UpdateAsync(entity, ct);
            return Result<IncidentResultDTO>.Ok(MapToResult(entity));
        }
        catch (Exception ex)
        {
            return Result<IncidentResultDTO>.Fail($"Error assigning incident: {ex.Message}");
        }
    }

    public async Task<Result<IncidentResultDTO>> ChangeStatusAsync(int id, ChangeStatusRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        try
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity is null)
                return Result<IncidentResultDTO>.Fail("Incident not found");

            entity.Update(status: req.Status);

            await _repository.UpdateAsync(entity, ct);
            return Result<IncidentResultDTO>.Ok(MapToResult(entity));
        }
        catch (Exception ex)
        {
            return Result<IncidentResultDTO>.Fail($"Error changing incident status: {ex.Message}");
        }
    }

    public async Task<Result<IncidentCommentDTO>> AddCommentAsync(int id, ClaimsPrincipal user, AddCommentRequest req, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(req);

        try
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity is null)
                return Result<IncidentCommentDTO>.Fail("Incident not found");

            var comment = IncidentComment.Create(entity.Id, req.Content, GetUserName(user));
            entity.Comments.Add(comment);

            await _repository.UpdateAsync(entity, ct);
            return Result<IncidentCommentDTO>.Ok(new IncidentCommentDTO
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorName = comment.AuthorName,
                CreatedAt = comment.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return Result<IncidentCommentDTO>.Fail($"Error adding comment: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CloseAsync(int id, ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity is null)
                return Result<bool>.Fail("Incident not found");

            entity.Update(status: "Closed");

            await _repository.UpdateAsync(entity, ct);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Error closing incident: {ex.Message}");
        }
    }

    public async Task<Result<IncidentStatsDTO>> GetStatsAsync(CancellationToken ct)
    {
        try
        {
            var entities = await _repository.ListAllAsync(ct);

            var active = entities.Where(i => i.Status is "Open" or "InProgress").ToList();
            var resolved = entities.Where(i => i.Status is "Resolved" or "Closed").ToList();

            var totalMttr = resolved
                .Where(i => i.ResolvedAt.HasValue && i.CreatedAt != default)
                .Select(i => (i.ResolvedAt!.Value - i.CreatedAt).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average();

            return Result<IncidentStatsDTO>.Ok(new IncidentStatsDTO
            {
                ActiveCount = active.Count,
                AverageMttr = (int)totalMttr,
                RecentLogs = entities
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .Select(i => new IncidentLogDTO
                    {
                        Id = $"INC-{i.Id:D4}",
                        Title = i.Title,
                        Phase = i.Status switch
                        {
                            "Open" => "CONTAINING",
                            "InProgress" => "INVESTIGATING",
                            "Resolved" => "MITIGATED",
                            "Closed" => "MITIGATED",
                            _ => "INVESTIGATING"
                        },
                        Time = i.CreatedAt.ToString("HH:mm") + " UTC"
                    })
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            return Result<IncidentStatsDTO>.Fail($"Error retrieving incident stats: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<RecentActivityDTO>>> GetRecentActivityAsync(ActivityFilterDTO filter, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);

        try
        {
            var entities = await _repository.ListAllAsync(ct);

            var activities = entities
                .Select(i => new RecentActivityDTO
                {
                    ActivityType = "incident",
                    Description = $"{i.Severity} incident: {i.Title}",
                    Timestamp = i.CreatedAt
                })
                .Union(entities
                    .SelectMany(i => i.Comments)
                    .Select(c => new RecentActivityDTO
                    {
                        ActivityType = "comment",
                        Description = $"Comment: {c.Content[..Math.Min(c.Content.Length, 50)]}",
                        Timestamp = c.CreatedAt
                    }))
                .Where(a => string.IsNullOrEmpty(filter.ActivityType) || a.ActivityType == filter.ActivityType)
                .Where(a => !filter.FromDate.HasValue || a.Timestamp >= filter.FromDate.Value)
                .Where(a => !filter.ToDate.HasValue || a.Timestamp <= filter.ToDate.Value)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            return Result<PagedResult<RecentActivityDTO>>.Ok(new PagedResult<RecentActivityDTO>
            {
                Items = activities.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = activities.Count,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return Result<PagedResult<RecentActivityDTO>>.Fail($"Error retrieving recent activities: {ex.Message}");
        }
    }

    private static IncidentResultDTO MapToResult(Incident entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        Severity = entity.Severity,
        Status = entity.Status,
        AssetId = entity.AssetId,
        AssignedToId = entity.AssignedToId,
        AssignedToName = entity.AssignedToName,
        Comments = entity.Comments.Select(c => new IncidentCommentDTO
        {
            Id = c.Id,
            Content = c.Content,
            AuthorName = c.AuthorName,
            CreatedAt = c.CreatedAt
        }).ToList(),
        CreatedAt = entity.CreatedAt,
        ResolvedAt = entity.ResolvedAt
    };

    private static string GetUserName(ClaimsPrincipal? user)
        => user?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
}