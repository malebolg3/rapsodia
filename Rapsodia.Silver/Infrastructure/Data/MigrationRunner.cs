// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Rapsodia.Silver.Infrastructure.Data;

public class MigrationRunner
{
    private static readonly Regex SafeHostRegex = new(@"^[a-zA-Z0-9_\-\.]+$", RegexOptions.Compiled);

    public static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationRunner>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            logger.LogInformation("Iniciando migracoes.");

            if (Environment.GetEnvironmentVariable("ENB_CONT") == "true")
            {
                var hostDb = Environment.GetEnvironmentVariable("HOST_DB") ?? "db";
                if (!SafeHostRegex.IsMatch(hostDb))
                    throw new ArgumentException("Nome de host invalido.");

                var connectionString = dbContext.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var updatedString = connectionString.Replace("localhost", hostDb).Replace("127.0.0.1", hostDb);
                    dbContext.Database.SetConnectionString(updatedString);
                }
            }

            dbContext.Database.Migrate();
            logger.LogInformation("Migracoes concluidas.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Falha na migracao.");
            throw;
        }
    }
}