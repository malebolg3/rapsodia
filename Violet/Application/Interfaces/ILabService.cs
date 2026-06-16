using Rapsodia.Violet.Application.DTOs;
using Rapsodia.Violet.Domain.Common;

namespace Rapsodia.Violet.Application.Interfaces;

public interface ILabService
{
    Task<Result<LabResultDTO>> CreateLabAsync(CreateLabRequest request, CancellationToken ct);
    Task<dynamic> UpdateLabAsync(string containerId, CreateLabRequest request, CancellationToken ct);
    Task<Result<LabResultDTO>> GetLabStatusAsync(string containerId, CancellationToken ct);
    Task<Result<List<LabResultDTO>>> ListLabsAsync(CancellationToken ct);
    Task<Result<bool>> StopLabAsync(string containerId, CancellationToken ct);
    Task<Result<bool>> StartLabAsync(string containerId, CancellationToken ct);
    Task<Result<bool>> RemoveLabAsync(string containerId, CancellationToken ct);
    Task<Result<LabSnapshotDTO>> CreateSnapshotAsync(string containerId, string snapshotName, CancellationToken ct);
    Task<Result<bool>> RestoreSnapshotAsync(string containerId, string snapshotId, CancellationToken ct);
    Task<Result<string>> ExecuteCommandAsync(string containerId, string command, CancellationToken ct);
}
