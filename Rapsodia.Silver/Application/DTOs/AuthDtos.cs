// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Rapsodia.Silver.Application.DTOs;

public record LoginRequest(
    [property: Required, StringLength(50, MinimumLength = 3)]
    [property: RegularExpression(@"^[a-zA-Z0-9_\-\.]+$")]
    [property: JsonPropertyName("username")]
    string Username,

    [property: Required, StringLength(128, MinimumLength = 8)]
    [property: JsonPropertyName("password")]
    string Password
);

public record RegisterRequest(
    [property: Required, StringLength(100, MinimumLength = 2)]
    [property: RegularExpression(@"^[a-zA-Z0-9\s]+$")]
    [property: JsonPropertyName("nome")]
    string Nome,

    [property: Required, StringLength(50, MinimumLength = 6)]
    [property: RegularExpression(@"^[a-zA-Z0-9_\-\.]+$")]
    [property: JsonPropertyName("username")]
    string Username,

    [property: Required, StringLength(128, MinimumLength = 12)]
    [property: JsonPropertyName("senha")]
    string Senha
);

public record AuthUserDto(string Id, string Nome, string Username);
public record AuthResponseDto(string Token, DateTime ExpiraEm, AuthUserDto Usuario);
public sealed record AuthTokenResult(string Token, DateTime ExpiresAtUtc, string Sub, string[] Roles);