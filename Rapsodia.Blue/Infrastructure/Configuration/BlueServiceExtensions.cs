// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Oracle.ManagedDataAccess.Client;
using Rapsodia.Blue.Infrastructure.Data;
using Rapsodia.Blue.Infrastructure.Data.Providers;

namespace Rapsodia.Blue.Infrastructure.Configuration;

public static class BlueServiceExtensions
{
    public static IServiceCollection AddBlueDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dbProv = Environment.GetEnvironmentVariable("DB_PROV") ?? configuration["DB_PROV"] ?? "Oracle";
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("DB_CONNECTION ausente.");

        if (dbProv.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
        {
            var tnsAdmin = Environment.GetEnvironmentVariable("TNS_ADMIN") ?? "/app/wallet";
            OracleConfiguration.TnsAdmin = tnsAdmin;
            OracleConfiguration.WalletLocation = tnsAdmin;
        }

        services.AddDbContext<BlueDbContext>((sp, options) =>
        {
            if (DatabaseToggle.UseSqlite || dbProv.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                var offlinePath = Environment.GetEnvironmentVariable("DB_PATH_OFFLINE") ?? "Data Source=rapsodia_offline.db";
                ProviderFactory.Create("SQLite").Configure(options, offlinePath);
            }
            else
            {
                ProviderFactory.Create(dbProv).Configure(options, connectionString);
            }
        });

        if (!DatabaseToggle.UseSqlite)
        {
            services.AddHostedService<DatabaseHealthService>();
        }

        return services;
    }

    public static IServiceCollection AddBlueServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddBlueHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IServiceCollection AddBlueSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Blue API",
                Version = "v1",
                Description = "Rapsodia Blue Shield API"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}