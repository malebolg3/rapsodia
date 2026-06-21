// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface INotificationService
{
    Task<Result<PagedResult<NotificationResultDTO>>> ListAsync(ClaimsPrincipal user, NotificationFilterDTO filter, CancellationToken ct);
    Task<Result<NotificationResultDTO>> GetByIdAsync(ClaimsPrincipal user, int id, CancellationToken ct);
    Task<Result<bool>> MarkAsReadAsync(ClaimsPrincipal user, int id, CancellationToken ct);
    Task<Result<bool>> MarkAllAsReadAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(ClaimsPrincipal user, int id, CancellationToken ct);
    Task<Result<UnreadCountDTO>> GetUnreadCountAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<bool>> UpdatePreferencesAsync(ClaimsPrincipal user, NotificationPreferencesRequest request, CancellationToken ct);
}