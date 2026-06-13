namespace Rapsodia.Silver.Spart.Interfaces;

public interface IRedAgent : IGrainWithIntegerKey
{
    Task<string> GetStatusAsync();
    Task<string> StartScanAsync(string target, string scanType);
    Task<string> StartExploitAsync(string target, string exploitName, Dictionary<string, object>? payload);
    Task<string> GetScanResultAsync(string scanId);
}