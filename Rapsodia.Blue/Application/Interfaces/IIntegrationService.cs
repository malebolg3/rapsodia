// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IIntegrationService
{
    Task<Result<IntegrationResultDTO>> CreateAsync(CreateIntegrationRequest request, ClaimsPrincipal user, CancellationToken ct);
    Task<Result<IntegrationResultDTO>> GetByIdAsync(int id, ClaimsPrincipal user, CancellationToken ct);
    Task<Result<List<IntegrationResultDTO>>> ListAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<IntegrationResultDTO>> UpdateAsync(int id, ClaimsPrincipal user, EditIntegrationRequest request, CancellationToken ct);
    Task<Result<bool>> TestConnectionAsync(int id, ClaimsPrincipal user, CancellationToken ct);
    Task<Result<IntegrationResultDTO>> ToggleAsync(int id, ClaimsPrincipal user, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, ClaimsPrincipal user, CancellationToken ct);
    Task<Result<PagedResult<IntegrationLogDTO>>> GetLogsAsync(int id, LogFilterDTO filter, ClaimsPrincipal user, CancellationToken ct);
}