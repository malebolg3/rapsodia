using Rapsodia.Red.Domain.Interfaces;
using Rapsodia.Red.Domain.Entities;

namespace Rapsodia.Red.Application.Services;

public interface ISecurityOrchestrator
{
    Task ExecuteWorkflowAsync(string target, CancellationToken ct);
}

public class SecurityOrchestrator : ISecurityOrchestrator
{
    private readonly IScanPort _scanPort;

    public SecurityOrchestrator(IScanPort scanPort)
    {
        _scanPort = scanPort;
    }

    public async Task ExecuteWorkflowAsync(string target, CancellationToken ct)
    {
        var scanTarget = ScanTarget.Create(target);
        await _scanPort.RunScanAsync(scanTarget);
    }
}