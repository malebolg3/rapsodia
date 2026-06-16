using Rapsodia.Violet.Application.DTOs;
using Rapsodia.Violet.Application.Interfaces;
using Rapsodia.Violet.Domain.Common;
using Rapsodia.Violet.Domain.Interfaces;

namespace Rapsodia.Violet.Application.Services;

public class LabService : ILabService
{
    private readonly ILabContainerPort _containerPort;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<LabService> _logger;

    public LabService(ILabContainerPort containerPort, IHttpClientFactory http, IConfiguration cfg, ILogger<LabService> logger)
    {
        _containerPort = containerPort;
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<Result<LabResultDTO>> CreateLabAsync(CreateLabRequest request, CancellationToken ct)
    {
        var lab = await _containerPort.CreateContainerAsync(request.Name, request.Image, request.Ports, request.EnvVars, request.TtlMinutes);
        _logger.LogInformation("Lab criado: {ContainerId} - {Name}", lab.ContainerId, lab.Name);
        await NotifyBlueAsync("lab_created", lab);
        
        return Result<LabResultDTO>.Ok(new LabResultDTO
        {
            ContainerId = lab.ContainerId,
            Name = lab.Name,
            Image = lab.Image,
            Status = lab.Status,
            IpAddress = lab.IpAddress,
            Ports = lab.Ports,
            CreatedAt = lab.CreatedAt,
            ExpiresAt = lab.ExpiresAt
        });
    }

    public async Task<dynamic> UpdateLabAsync(string containerId, CreateLabRequest request, CancellationToken ct)
    {
        await _containerPort.RemoveContainerAsync(containerId);
        var lab = await _containerPort.CreateContainerAsync(request.Name, request.Image, request.Ports, request.EnvVars, request.TtlMinutes);
        _logger.LogInformation("Lab atualizado: {OldId} -> {NewId} - {Name}", containerId, lab.ContainerId, lab.Name);
        await NotifyBlueAsync("lab_updated", lab);
        return lab;
    }

    public async Task<Result<LabResultDTO>> GetLabStatusAsync(string containerId, CancellationToken ct)
    {
        var lab = await _containerPort.GetContainerAsync(containerId);
        return lab is null 
            ? Result<LabResultDTO>.Fail("Lab not found") 
            : Result<LabResultDTO>.Ok(lab);
    }

    public async Task<Result<List<LabResultDTO>>> ListLabsAsync(CancellationToken ct)
    {
        var labs = await _containerPort.ListContainersAsync();
        return Result<List<LabResultDTO>>.Ok(labs);
    }

    public async Task<Result<bool>> StopLabAsync(string containerId, CancellationToken ct)
    {
        await _containerPort.StopContainerAsync(containerId);
        _logger.LogInformation("Lab parado: {ContainerId}", containerId);
        await NotifyBlueAsync("lab_stopped", new { containerId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> StartLabAsync(string containerId, CancellationToken ct)
    {
        await _containerPort.StartContainerAsync(containerId);
        _logger.LogInformation("Lab iniciado: {ContainerId}", containerId);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RemoveLabAsync(string containerId, CancellationToken ct)
    {
        await _containerPort.RemoveContainerAsync(containerId);
        _logger.LogInformation("Lab removido: {ContainerId}", containerId);
        await NotifyBlueAsync("lab_removed", new { containerId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<LabSnapshotDTO>> CreateSnapshotAsync(string containerId, string snapshotName, CancellationToken ct)
    {
        var snapshotId = await _containerPort.CreateSnapshotAsync(containerId, snapshotName);
        _logger.LogInformation("Snapshot criado: {SnapshotId} para {ContainerId}", snapshotId, containerId);
        return Result<LabSnapshotDTO>.Ok(new LabSnapshotDTO
        {
            SnapshotId = snapshotId,
            ContainerId = containerId,
            Name = snapshotName,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<Result<bool>> RestoreSnapshotAsync(string containerId, string snapshotId, CancellationToken ct)
    {
        await _containerPort.RestoreSnapshotAsync(containerId, snapshotId);
        _logger.LogInformation("Snapshot restaurado: {SnapshotId} para {ContainerId}", snapshotId, containerId);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<string>> ExecuteCommandAsync(string containerId, string command, CancellationToken ct)
    {
        var output = await _containerPort.ExecuteCommandAsync(containerId, command);
        return Result<string>.Ok(output);
    }

    private async Task NotifyBlueAsync(string eventType, object data)
    {
        try
        {
            var bluePort = _cfg["PORT_BLU"] ?? "5073";
            var client = _http.CreateClient();
            await client.PostAsJsonAsync($"http://localhost:{bluePort}/api/lab/events", new { eventType, data, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao notificar Blue sobre evento {EventType}", eventType);
        }
    }
}