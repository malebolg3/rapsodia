// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Infrastructure.Data.Providers;

namespace Rapsodia.Blue.Infrastructure.Data;

public static class DatabaseToggle
{
    private static volatile bool _useSqlite;
    private static volatile bool _autoFallback;
    private static readonly object Lock = new();

    public static bool UseSqlite
    {
        get => _useSqlite || _autoFallback;
        set { lock (Lock) { _useSqlite = value; _autoFallback = false; } }
    }

    public static bool IsAutoFallback => _autoFallback;

    public static void SetAutoFallback(bool fallback)
    {
        lock (Lock) { _autoFallback = fallback; }
    }
}