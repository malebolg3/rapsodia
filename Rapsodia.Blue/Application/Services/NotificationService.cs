// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class NotificationService : INotificationService
{
    private readonly List<NotificationResultDTO> _notifications = new();
    private readonly IConfiguration _cfg;
    private readonly bool _mock;
    private int _nextId = 1;

    public NotificationService(IConfiguration cfg)
    {
        _cfg = cfg;
        _mock = cfg["NTF_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DB_HOST"]);
        
        if (_mock)
        {
            _notifications.Add(new NotificationResultDTO
            {
                Id = _nextId++,
                Title = "New Vulnerability Found",
                Message = "Critical CVE-2024-1234 detected on Asset #42",
                Type = "vuln",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            });
            _notifications.Add(new NotificationResultDTO
            {
                Id = _nextId++,
                Title = "Scan Completed",
                Message = "Network scan finished with 15 findings",
                Type = "scan",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ReadAt = DateTime.UtcNow.AddHours(-12)
            });
            _notifications.Add(new NotificationResultDTO
            {
                Id = _nextId++,
                Title = "Incident Resolved",
                Message = "Incident #7 has been resolved by Admin",
                Type = "incident",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            });
        }
    }

    public Task<Result<PagedResult<NotificationResultDTO>>> ListAsync(ClaimsPrincipal user, NotificationFilterDTO filter, CancellationToken ct)
    {
        var userId = GetUserId(user);
        
        var items = _notifications
            .Where(n => string.IsNullOrEmpty(filter.Type) || n.Type == filter.Type)
            .Where(n => !filter.IsRead.HasValue || n.IsRead == filter.IsRead.Value)
            .ToList();

        return Task.FromResult(Result<PagedResult<NotificationResultDTO>>.Ok(new PagedResult<NotificationResultDTO>
        {
            Items = items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            TotalCount = items.Count,
            Page = filter.Page,
            PageSize = filter.PageSize
        }));
    }

    public Task<Result<NotificationResultDTO>> GetByIdAsync(ClaimsPrincipal user, int id, CancellationToken ct)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        return Task.FromResult(notification is null 
            ? Result<NotificationResultDTO>.Fail("Notification not found") 
            : Result<NotificationResultDTO>.Ok(notification));
    }

    public Task<Result<bool>> MarkAsReadAsync(ClaimsPrincipal user, int id, CancellationToken ct)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification is null)
            return Task.FromResult(Result<bool>.Fail("Notification not found"));

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<bool>> MarkAllAsReadAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        foreach (var n in _notifications.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<bool>> DeleteAsync(ClaimsPrincipal user, int id, CancellationToken ct)
    {
        _notifications.RemoveAll(n => n.Id == id);
        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<UnreadCountDTO>> GetUnreadCountAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var unread = _notifications.Where(n => !n.IsRead).ToList();
        return Task.FromResult(Result<UnreadCountDTO>.Ok(new UnreadCountDTO
        {
            Total = unread.Count,
            ByType = unread.GroupBy(n => n.Type).ToDictionary(g => g.Key, g => g.Count())
        }));
    }

    public Task<Result<bool>> UpdatePreferencesAsync(ClaimsPrincipal user, NotificationPreferencesRequest req, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    private static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? int.Parse(claim.Value) : 0;
    }
}