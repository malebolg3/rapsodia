using Rapsodia.Violet.Application.DTOs;

namespace Rapsodia.Violet.Domain.Interfaces;

public interface ILabContainerPort
{
    Task<LabResultDTO> CreateContainerAsync(string name, string image, List<string>? ports, Dictionary<string, string>? envVars, int ttlMinutes);
    Task<LabResultDTO?> GetContainerAsync(string containerId);
    Task<List<LabResultDTO>> ListContainersAsync();
    Task StopContainerAsync(string containerId);
    Task StartContainerAsync(string containerId);
    Task RemoveContainerAsync(string containerId);
    Task<string> CreateSnapshotAsync(string containerId, string snapshotName);
    Task RestoreSnapshotAsync(string containerId, string snapshotId);
    Task<string> ExecuteCommandAsync(string containerId, string command);
}
