// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;

namespace Rapsodia.Blue.Infrastructure.Data.Providers;

public class OracleProvider : IDatabaseProvider
{
    public void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseOracle(connectionString, oracleOptions =>
        {
            oracleOptions.CommandTimeout(60);
        });
    }
}