// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Rapsodia.Silver.Spart;

public class OrleansHostedService : IHostedService
{
    private readonly ILogger<OrleansHostedService> _logger;

    public OrleansHostedService(ILogger<OrleansHostedService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var orleans = Environment.GetEnvironmentVariable("ORLEANS") == "true";
        if (orleans)
        {
            _logger.LogInformation("Orleans cluster iniciado");
        }
        else
        {
            _logger.LogInformation("Orleans desabilitado");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Orleans cluster parado");
        return Task.CompletedTask;
    }
}