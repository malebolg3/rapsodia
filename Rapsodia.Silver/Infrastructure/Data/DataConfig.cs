using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Rapsodia.Silver.Infrastructure.Data;

public static class DataConfig
{
    private static readonly Regex SafeNameRegex = new(@"^[a-zA-Z0-9_\-\.]+$", RegexOptions.Compiled);

    public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration, string schema = "principal")
    {   
        var assemblyName = Environment.GetEnvironmentVariable("DB_ASSEMBLY") ?? configuration["DB_ASSEMBLY"];
        var methodName = Environment.GetEnvironmentVariable("DB_METHOD") ?? configuration["DB_METHOD"];
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("DB_ASSEMBLY, DB_METHOD, DB_CONNECTION ausentes.");

        if (!SafeNameRegex.IsMatch(assemblyName) || !SafeNameRegex.IsMatch(methodName))
            throw new ArgumentException("Assinatura de provedor invalida detectada.");

        services.AddDbContext<AppDbContext>(options =>
            ConfigureProvider(options, assemblyName, methodName, connectionString),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        ConfigureCache(services, configuration);
        return services;
    }

    public static void ConfigureProvider(DbContextOptionsBuilder options, string assemblyName, string methodName, string connectionString)
    {
        var assembly = Assembly.Load(assemblyName);
        var method = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length >= 2 && m.GetParameters()[0].ParameterType == typeof(DbContextOptionsBuilder));

        if (method == null) 
            throw new InvalidOperationException($"Metodo {methodName} nao encontrado no assembly {assemblyName}.");

        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];
        args[0] = options;
        args[1] = connectionString;

        if (parameters.Length > 2 && parameters[2].ParameterType.Name == "ServerVersion")
        {
            var serverVersionType = assembly.GetType("Microsoft.EntityFrameworkCore.ServerVersion") ?? assembly.GetType("Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerVersion");
            var autoDetect = serverVersionType?.GetMethod("AutoDetect", new[] { typeof(string) });
            args[2] = autoDetect?.Invoke(null, new object[] { connectionString });
        }

        method.Invoke(null, args);
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