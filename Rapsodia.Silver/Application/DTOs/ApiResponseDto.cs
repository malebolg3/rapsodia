// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Application.DTOs;

public record ApiResponse<T>(bool Success, T? Data, string[]? Errors, string? TraceId = null)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);
    public static ApiResponse<T> Fail(string[] errors, string traceId) => new(false, default, errors, traceId);
}