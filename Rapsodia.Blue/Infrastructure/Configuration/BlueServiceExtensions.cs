// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Oracle.ManagedDataAccess.Client;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Infrastructure.Configuration;

public static class BlueServiceExtensions
{
    public static IServiceCollection AddBlueDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = (Environment.GetEnvironmentVariable("DB_PROV") ?? "InMemory").Replace("\"", "").Trim();
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "1522";
        var name = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var pass = Environment.GetEnvironmentVariable("DB_PASS");

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") 
            ?? $"User Id={user};Password={pass};Data Source={host}:{port}/{name}";
            
        var tnsAdmin = Environment.GetEnvironmentVariable("TNS_ADMIN") ?? "/app/wallet";
        Console.WriteLine($"[DB] INICIANDO AddBlueDatabase com provider={provider}, TNS_ADMIN={tnsAdmin}");
        Console.WriteLine($"[DB] ConnectionString: {connectionString}");

        if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
        {
            OracleConfiguration.TnsAdmin = tnsAdmin;
            OracleConfiguration.WalletLocation = tnsAdmin;
            Console.WriteLine("[DB] OracleConfiguration definida");
        }

        services.AddDbContext<BlueDbContext>(options =>
        {
            if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
            {
                options.UseOracle(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("RapsodiaBlue");
            }
        });

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