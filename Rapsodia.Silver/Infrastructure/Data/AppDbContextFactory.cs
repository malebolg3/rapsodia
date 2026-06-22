// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Infrastructure.Providers;

namespace Rapsodia.Silver.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var schema = args.Contains("--labs") ? "labs" : "principal";
        var dbProv = Environment.GetEnvironmentVariable("DB_PROV") ?? config["DB_PROV"] ?? "Oracle";
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("DB_CONNECTION ausente.");

        IDatabaseProvider provider = dbProv switch
        {
            "Oracle" => new OracleProvider(),
            "SQLite" => new SqliteProvider(),
            _ => throw new ArgumentException($"Provider não suportado: {dbProv}")
        };

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        provider.Configure(optionsBuilder, connectionString);

        return new AppDbContext(optionsBuilder.Options, schema: schema);
    }
}