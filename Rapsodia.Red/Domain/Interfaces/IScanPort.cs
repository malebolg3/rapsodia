using Rapsodia.Red.Domain.Entities;

namespace Rapsodia.Red.Domain.Interfaces;

public interface IScanPort
{
    Task<bool> RunScanAsync(ScanTarget target);
}