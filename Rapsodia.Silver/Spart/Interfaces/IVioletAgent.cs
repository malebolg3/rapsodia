namespace Rapsodia.Silver.Spart.Interfaces;
using Rapsodia.Silver.Spart.Grains;

public interface IVioletAgent : IGrainWithIntegerKey
{
    Task<string> GetStatusAsync();
    Task<string> CreateLabAsync(string name, string image);
    Task<string> CreateSandboxAsync(string name, string image, List<string> tools, int ttlMinutes = 240);
    Task<string> DeployHoneypotAsync(string name, string honeypotType, int ttlMinutes = 480);
    Task<string> DeploySOCAsync(string name, int ttlMinutes = 480);
    Task<string> DeployCyberRangeAsync(string name, string scenario, int ttlMinutes = 1440);
    Task<bool> DestroyLabAsync(string labId);
    Task<string> GetLabStatusAsync(string labId);
    Task<List<LabInfo>> ListLabsAsync();
    Task<int> CleanupExpiredAsync();
}