namespace Rapsodia.Silver.Spart.Interfaces;

public interface ISilverAgent : IGrainWithIntegerKey
{
    Task<string> AnalyzeAsync(string input);
    Task<string> GetStatusAsync();
    Task SetContextAsync(string context);
}