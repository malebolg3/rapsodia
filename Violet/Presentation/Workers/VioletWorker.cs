using Microsoft.Extensions.Logging;

namespace Rapsodia.Violet.Presentation.Workers;

public class VioletWorker : BackgroundService
{
    private readonly ILogger<VioletWorker> _logger;

    public VioletWorker(ILogger<VioletWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Violet Worker iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Monitora labs ativos
                _logger.LogInformation("Labs monitorados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no monitoramento de labs");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Violet Worker encerrado");
    }
}