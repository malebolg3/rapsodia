// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Silver.Domain.Interfaces;

namespace Rapsodia.Silver.Infrastructure.Providers;

public class OracleProvider : IDatabaseProvider
{
    public void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseOracle(connectionString);
    }
}