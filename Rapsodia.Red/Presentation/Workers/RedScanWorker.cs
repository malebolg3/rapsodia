using Rapsodia.Red.Application.Services;

namespace Rapsodia.Red.Presentation.Workers;

public class RedScanWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RedScanWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<ISecurityOrchestrator>();
            await orchestrator.ExecuteWorkflowAsync("127.0.0.1", ct);
            await Task.Delay(60000, ct);
        }
    }
}
