// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;

namespace Rapsodia.Blue.Infrastructure.Data.Providers;

public interface IDatabaseProvider
{
    void Configure(DbContextOptionsBuilder options, string connectionString);
}