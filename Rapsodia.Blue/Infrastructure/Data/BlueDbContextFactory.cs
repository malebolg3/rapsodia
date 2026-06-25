// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Rapsodia.Blue.Infrastructure.Data.Providers;

namespace Rapsodia.Blue.Infrastructure.Data;

public class BlueDbContextFactory : IDesignTimeDbContextFactory<BlueDbContext>
{
    public BlueDbContext CreateDbContext(string[] args)
    {
        var dbProv = Environment.GetEnvironmentVariable("DB_PROV") ?? "Oracle";
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
            ?? "User Id=MALEBOLGE;Password=;Data Source=127.0.0.1:1522/malebolge_low";

        var optionsBuilder = new DbContextOptionsBuilder<BlueDbContext>();
        ProviderFactory.Create(dbProv).Configure(optionsBuilder, connectionString);

        return new BlueDbContext(optionsBuilder.Options);
    }
}