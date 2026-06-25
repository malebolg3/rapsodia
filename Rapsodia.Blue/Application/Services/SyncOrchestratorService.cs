// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Infrastructure.Data;
using Rapsodia.Blue.Infrastructure.Data.Providers;

namespace Rapsodia.Blue.Application.Services;

public class SyncOrchestratorService
{
    private readonly ISyncRepositoryPort _syncRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncOrchestratorService> _logger;

    public SyncOrchestratorService(
        ISyncRepositoryPort syncRepository,
        IServiceProvider serviceProvider,
        ILogger<SyncOrchestratorService> logger)
    {
        _syncRepository = syncRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        if (DatabaseToggle.UseSqlite) return;

        var pending = await _syncRepository.GetPendingAsync(50, ct);
        if (!pending.Any()) return;

        using var scope = _serviceProvider.CreateScope();
        var targetDb = scope.ServiceProvider.GetRequiredService<BlueDbContext>();

        foreach (var item in pending)
        {
            try
            {
                await ApplyToTarget(item, targetDb, ct);
                item.MarkAsSynced();
            }
            catch (Exception ex)
            {
                item.RecordFailure(ex.Message);
                _logger.LogWarning(ex, "Falha ao sincronizar {EntityType}:{EntityId}", item.EntityType, item.EntityId);
            }

            await _syncRepository.UpdateAsync(item, ct);
        }
    }

    private async Task ApplyToTarget(SyncQueue item, BlueDbContext targetDb, CancellationToken ct)
    {
        var entityType = Type.GetType($"Rapsodia.Blue.Domain.Entities.{item.EntityType}")
                      ?? Type.GetType($"Rapsodia.Blue.Domain.Entities.Olimpo.{item.EntityType}");

        if (entityType == null)
            throw new InvalidOperationException($"Tipo {item.EntityType} não encontrado");

        switch (item.Operation)
        {
            case "INSERT":
                var newEntity = JsonSerializer.Deserialize(item.Payload, entityType);
                if (newEntity != null)
                {
                    targetDb.Add(newEntity);
                    await targetDb.SaveChangesAsync(ct);
                }
                break;

            case "UPDATE":
                var existing = await targetDb.FindAsync(entityType, new object[] { item.EntityId }, ct);
                var updatedEntity = JsonSerializer.Deserialize(item.Payload, entityType);
                if (existing != null && updatedEntity != null)
                {
                    targetDb.Entry(existing).CurrentValues.SetValues(updatedEntity);
                    await targetDb.SaveChangesAsync(ct);
                }
                break;

            case "DELETE":
                var toDelete = await targetDb.FindAsync(entityType, new object[] { item.EntityId }, ct);
                if (toDelete != null)
                {
                    targetDb.Remove(toDelete);
                    await targetDb.SaveChangesAsync(ct);
                }
                break;

            default:
                throw new InvalidOperationException($"Operação desconhecida: {item.Operation}");
        }
    }
}