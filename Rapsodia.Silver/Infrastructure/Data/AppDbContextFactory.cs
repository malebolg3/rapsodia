// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Rapsodia.Silver.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private static readonly Regex SafeNameRegex = new(@"^[a-zA-Z0-9_\-\.]+$", RegexOptions.Compiled);

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
        var providerAssembly = Environment.GetEnvironmentVariable("DB_ASSEMBLY") ?? config["DB_ASSEMBLY"];
        var providerMethod = Environment.GetEnvironmentVariable("DB_METHOD") ?? config["DB_METHOD"];
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(providerAssembly) || string.IsNullOrEmpty(providerMethod) || string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Configuracoes de banco incompletas.");

        if (!SafeNameRegex.IsMatch(providerAssembly) || !SafeNameRegex.IsMatch(providerMethod))
            throw new ArgumentException("Assinatura de provedor invalida.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        DataConfig.ConfigureProvider(optionsBuilder, providerAssembly, providerMethod, connectionString);

        return new AppDbContext(optionsBuilder.Options, schema: schema);
    }
}