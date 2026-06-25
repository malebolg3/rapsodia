// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Infrastructure.Data.Providers;

namespace Rapsodia.Blue.Infrastructure.Data;

public class SyncSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        EnqueueChanges(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    private static void EnqueueChanges(DbContext? context)
    {
        if (context is not BlueDbContext db || !DatabaseToggle.UseSqlite)
            return;

        var entries = db.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity is not SyncQueue && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var operation = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };

            if (operation == null) continue;

            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Property(nameof(BaseEntity.Id)).CurrentValue?.ToString() ?? "0";

            var syncItem = new SyncQueue(
                entityType,
                entityId,
                operation,
                JsonSerializer.Serialize(entry.Entity));

            db.Set<SyncQueue>().Add(syncItem);
        }
    }
}