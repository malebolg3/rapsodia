namespace Rapsodia.Silver.Spart.Interfaces;

public interface IBlueAgent : IGrainWithIntegerKey
{
    Task<string> GetStatusAsync();
    Task TrackScanAsync(string scanId, string target);
    Task TrackVulnAsync(string vulnId, string severity);
    Task<string> GetAssetSummaryAsync();
    Task NotifyIncidentAsync(string incidentId, string title, string severity);
}