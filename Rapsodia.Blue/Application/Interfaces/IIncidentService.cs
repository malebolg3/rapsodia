// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IIncidentService
{
    Task<Result<IncidentResultDTO>> CreateAsync(ClaimsPrincipal user, CreateIncidentRequest request, CancellationToken ct);
    Task<Result<IncidentResultDTO>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<PagedResult<IncidentResultDTO>>> ListAsync(IncidentFilterDTO filter, CancellationToken ct);
    Task<Result<IncidentResultDTO>> UpdateAsync(int id, EditIncidentRequest request, CancellationToken ct);
    Task<Result<IncidentResultDTO>> AssignAsync(int id, AssignIncidentRequest request, CancellationToken ct);
    Task<Result<IncidentResultDTO>> ChangeStatusAsync(int id, ChangeStatusRequest request, CancellationToken ct);
    Task<Result<IncidentCommentDTO>> AddCommentAsync(int id, ClaimsPrincipal user, AddCommentRequest request, CancellationToken ct);
    Task<Result<bool>> CloseAsync(int id, ClaimsPrincipal user, CancellationToken ct);
    Task<Result<IncidentStatsDTO>> GetStatsAsync(CancellationToken ct);
    Task<Result<PagedResult<RecentActivityDTO>>> GetRecentActivityAsync(ActivityFilterDTO filter, CancellationToken ct);
}