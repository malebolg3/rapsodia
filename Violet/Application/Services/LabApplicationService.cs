// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Violet.Application.DTOs;
using Rapsodia.Violet.Domain.Interfaces;

namespace Rapsodia.Violet.Application.Services;

public class LabApplicationService
{
    private readonly ILabContainerPort _container;
    private readonly ILogger<LabApplicationService> _logger;

    public LabApplicationService(ILabContainerPort container, ILogger<LabApplicationService> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<List<LabResultDTO>> ListActiveLabsAsync(CancellationToken ct)
    {
        var labs = await _container.ListContainersAsync();
        return labs.Where(l => l.Status == "running").ToList();
    }

    public async Task<LabResultDTO> CreateLabAsync(string name, string image, CancellationToken ct)
    {
        return await _container.CreateContainerAsync(name, image, null, null, 60);
    }

    public async Task<bool> DestroyLabAsync(string labId, CancellationToken ct)
    {
        await _container.RemoveContainerAsync(labId);
        return true;
    }

    public async Task<bool> MonitorLabAsync(string labId, CancellationToken ct)
    {
        var lab = await _container.GetContainerAsync(labId);
        return lab?.Status == "running";
    }

    public async Task<List<LabResultDTO>> CleanupExpiredLabsAsync(CancellationToken ct)
    {
        var labs = await _container.ListContainersAsync();
        var expired = labs.Where(l => l.ExpiresAt.HasValue && l.ExpiresAt < DateTime.UtcNow).ToList();
        
        foreach (var lab in expired)
        {
            _logger.LogInformation("Removendo lab expirado: {ContainerId} - {Name}", lab.ContainerId, lab.Name);
            await _container.RemoveContainerAsync(lab.ContainerId);
        }
        
        return expired;
    }
}
