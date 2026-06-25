// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Infrastructure.Data.Providers;

public static class ProviderFactory
{
    public static IDatabaseProvider Create(string providerName) => providerName.ToLower() switch
    {
        "oracle" => new OracleProvider(),
        "sqlite" => new SqliteProvider(),
        _ => throw new ArgumentException($"Provider inválido: {providerName}")
    };
}