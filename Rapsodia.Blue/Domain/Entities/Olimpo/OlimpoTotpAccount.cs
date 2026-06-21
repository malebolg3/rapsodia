// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Domain.Entities.Olimpo;

public sealed class OlimpoTotpAccount
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Uri { get; private set; } = string.Empty;
    public string Secret { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string Issuer { get; private set; } = string.Empty;
    public string Algorithm { get; private set; } = string.Empty;
    public int Digits { get; private set; }
    public int Period { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OlimpoTotpAccount() { }

    public static OlimpoTotpAccount Create(Guid userId, string uri, string secret, string label, string issuer, string algorithm = "SHA1", int digits = 6, int period = 30)
    {
        return new OlimpoTotpAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Uri = uri,
            Secret = secret,
            Label = label,
            Issuer = issuer,
            Algorithm = algorithm,
            Digits = digits,
            Period = period,
            CreatedAt = DateTime.UtcNow
        };
    }
}