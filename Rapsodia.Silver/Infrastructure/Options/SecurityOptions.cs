// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Infrastructure.Options;

public class SecurityOptions
{
    public const string SectionName = "Security";
    public string FieldEncryptionKey { get; set; } = string.Empty;
    public string AgentApiKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(FieldEncryptionKey) || FieldEncryptionKey.Length < 32)
            throw new InvalidOperationException("FieldEncryptionKey inválida (mínimo 32 caracteres)");
        if (string.IsNullOrWhiteSpace(AgentApiKey))
            throw new InvalidOperationException("AgentApiKey ausente");
    }
}