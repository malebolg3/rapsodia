// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Domain.Interfaces;
using Rapsodia.Silver.Infrastructure.Providers;
using StackExchange.Redis;

namespace Rapsodia.Silver.Infrastructure.Data;

public static class DataConfig
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration, string schema = "principal")
    {
        var dbProv = Environment.GetEnvironmentVariable("DB_PROV") ?? configuration["DB_PROV"] ?? "Oracle";
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("DB_CONNECTION ausente.");

        services.AddSingleton<IDatabaseProvider>(_ => dbProv switch
        {
            "Oracle" => new OracleProvider(),
            "SQLite" => new SqliteProvider(),
            _ => throw new ArgumentException($"Provider não suportado: {dbProv}")
        });

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var provider = sp.GetRequiredService<IDatabaseProvider>();
            provider.Configure(options, connectionString);
        }, ServiceLifetime.Transient, ServiceLifetime.Transient);

        ConfigureCache(services, configuration);
        return services;
    }

    private static void ConfigureCache(IServiceCollection services, IConfiguration configuration)
    {
        var enbCache = Environment.GetEnvironmentVariable("CCH_ENB") ?? configuration["CCH_ENB"];
        if (enbCache != "true")
        {
            services.AddSingleton<IDatabase>(sp => null!);
            return;
        }

        var cnxCache = Environment.GetEnvironmentVariable("CCH_URL") ?? configuration["CCH_URL"] ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(new ConfigurationOptions
                {
                    EndPoints = { cnxCache },
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,
                    SyncTimeout = 5000
                });
            }
            catch (Exception ex)
            {
                sp.GetRequiredService<ILogger<AppDbContext>>().LogWarning(ex, "Cache Redis indisponivel.");
                return null!;
            }
        });

        services.AddSingleton<IDatabase>(sp => sp.GetService<IConnectionMultiplexer>()?.GetDatabase()!);
    }
}