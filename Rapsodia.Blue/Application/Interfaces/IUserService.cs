// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResultDTO>> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<Result<UserResultDTO>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<PagedResult<UserResultDTO>>> ListAsync(UserFilterDTO filter, CancellationToken ct);
    Task<Result<UserResultDTO>> UpdateAsync(int id, EditUserRequest request, CancellationToken ct);
    Task<Result<bool>> DisableAsync(int id, CancellationToken ct);
    Task<Result<bool>> EnableAsync(int id, CancellationToken ct);
    Task<Result<UserResultDTO>> SetPermissionsAsync(int id, string role, string allowedModules, CancellationToken ct);
}