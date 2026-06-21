// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.DTOs;

public record ResponseModel<T>(T? Data, bool Success, string Message)
{
    public static ResponseModel<T> CreateSuccess(T data, string message = "") 
        => new(data, true, message);
    public static ResponseModel<T> CreateError(string message) 
        => new(default, false, message);
    
    public static ResponseModel<T> FromResult(Result<T> result)
        => result.Success 
            ? new(result.Data, true, result.Message ?? "")
            : new(default, false, result.Message ?? "");
}