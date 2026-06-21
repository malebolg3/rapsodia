// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.DTOs.Auth;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResultDTO>> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<Result<AuthResultDTO>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result<AuthResultDTO>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct);
    Task<Result<bool>> LogoutAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<AuthResultDTO>> GetCurrentUserAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<AuthResultDTO>> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileRequest request, CancellationToken ct);
    Task<Result<bool>> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordRequest request, CancellationToken ct);
    Task<Result<AuthorizeResponse>> RequestAuthorizationAsync(AuthorizeRequest request, CancellationToken ct);
    Task<Result<SessionInfo>> Verify2FAAsync(Verify2FARequest request, CancellationToken ct);
    Task<Result<List<SessionInfo>>> GetActiveSessionsAsync(CancellationToken ct);
    Task<Result<bool>> RevokeSessionAsync(string sessionId, CancellationToken ct);
}