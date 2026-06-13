namespace Rapsodia.Red.Domain.Interfaces;

public interface IAiAnalystPort
{
    Task<string> AnalyzeScanResultsAsync(string toolName, string rawOutput);
}
