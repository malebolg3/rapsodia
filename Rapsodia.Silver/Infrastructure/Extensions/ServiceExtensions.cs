// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Rapsodia.Silver.Infrastructure.Data;
using Rapsodia.Silver.Infrastructure.Services;
using Rapsodia.Silver.Application.Interfaces;

namespace Rapsodia.Silver.Infrastructure.Extensions;

public static class ServiceExtensions
{
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    private static readonly ILogger _logger = _loggerFactory.CreateLogger("ServiceExtensions");

    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env)
    {
        services.AddControllers()
            .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        services.AddHttpClient<AiClient>();

        var blueUrl = Environment.GetEnvironmentVariable("BLUE_URL") ?? "http://localhost:5073";
        services.AddHttpClient("BlueClient", client =>
        {
            client.BaseAddress = new Uri(blueUrl);
        });

        ConfigureRateLimiting(services);
        ConfigureJwt(services, config);
        ConfigureCors(services, config);
        ConfigureDatabase(services, config);
        ConfigureCache(services, config);

        return services;
    }

    public static IServiceCollection AddAppServices(
        this IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env)
    {
        AddCoreServices(services, config, env);

        if (env.IsDevelopment())
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Silver API", Version = "v1" });
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
        }

        return services;
    }

    private static void ConfigureRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            o.AddFixedWindowLimiter("Global", l =>
            {
                l.Window = TimeSpan.FromMinutes(5);
                l.PermitLimit = 100;
            });

            o.AddFixedWindowLimiter("Auth", l =>
            {
                l.Window = TimeSpan.FromMinutes(5);
                l.PermitLimit = 3;
                l.QueueLimit = 0;
            });

            o.AddFixedWindowLimiter("Api", l =>
            {
                l.Window = TimeSpan.FromMinutes(1);
                l.PermitLimit = 50;
                l.QueueLimit = 10;
            });

            o.OnRejected = async (ctx, t) =>
            {
                ctx.HttpContext.Response.StatusCode = 429;
                await ctx.HttpContext.Response.WriteAsJsonAsync(
                    new { Status = false, Mensagem = "Limite excedido." }, t);
            };
        });
    }

    private static void ConfigureJwt(IServiceCollection services, IConfiguration config)
    {
        var jwtKey = Environment.GetEnvironmentVariable("AUTH_KEY")
            ?? throw new InvalidOperationException("AUTH_KEY ausente.");

        var jwtIss = Environment.GetEnvironmentVariable("AUTH_ISS")
            ?? throw new InvalidOperationException("AUTH_ISS ausente.");

        var jwtAud = Environment.GetEnvironmentVariable("AUTH_AUD")
            ?? throw new InvalidOperationException("AUTH_AUD ausente.");

        var authMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "jwt";

        if (authMode == "standalone")
        {
            return;
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIss,
                    ValidateAudience = true,
                    ValidAudience = jwtAud,
                    ValidateLifetime = true
                };
            });

        services.AddAuthorization();
    }

    private static void ConfigureCors(IServiceCollection services, IConfiguration config)
    {
        var origins = Environment.GetEnvironmentVariable("CORS")
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .ToArray()
            ?? config.GetSection("CORS").Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(o => o.AddPolicy("DefaultPolicy", p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration config)
    {
        var enbCore = Environment.GetEnvironmentVariable("ENB_SLV") ?? config["ENB_SLV"];
        if (enbCore == "true")
        {
            DataConfig.AddDataContext(services, config);
        }
    }

    private static void ConfigureCache(IServiceCollection services, IConfiguration config)
    {
        var enbCache = Environment.GetEnvironmentVariable("CCH_ENB") ?? config["CCH_ENB"];
        if (enbCache != "true")
        {
            services.AddSingleton<ICacheService, NullCacheService>();
            return;
        }

        var cnxCache = Environment.GetEnvironmentVariable("CCH_URL") ?? config["CCH_URL"] ?? "localhost:6379";
        try
        {
            var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { cnxCache },
                AbortOnConnectFail = false,
                ConnectTimeout = 5000
            });
            services.AddSingleton<IConnectionMultiplexer>(redis);
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache indisponivel. Usando NullCache.");
            services.AddSingleton<ICacheService, NullCacheService>();
        }
    }
}