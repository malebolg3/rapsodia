// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key) || Key.Length < 32)
            throw new InvalidOperationException("JWT_KEY inválida (mínimo 32 caracteres)");
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT_ISSUER ausente");
        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT_AUDIENCE ausente");
    }
}