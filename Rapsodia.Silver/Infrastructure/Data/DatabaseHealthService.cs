// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Rapsodia.Silver.Infrastructure.Data;

public class DatabaseHealthService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<DatabaseHealthService> _logger;
    private int _failures;

    public DatabaseHealthService(IServiceProvider sp, ILogger<DatabaseHealthService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (DatabaseToggle.UseSqlite) return;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.ExecuteSqlRawAsync("SELECT 1 FROM DUAL", ct);
                _failures = 0;
                if (DatabaseToggle.IsAutoFallback)
                {
                    DatabaseToggle.SetAutoFallback(false);
                    _logger.LogWarning("Oracle recuperado. Voltando ao modo online.");
                }
            }
            catch
            {
                _failures++;
                if (_failures >= 3 && !DatabaseToggle.IsAutoFallback)
                {
                    DatabaseToggle.SetAutoFallback(true);
                    _logger.LogWarning("Oracle indisponivel. Ativando SQLite automatico.");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), ct);
        }
    }
}