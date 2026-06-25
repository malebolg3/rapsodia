// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Domain.Enums;

namespace Rapsodia.Blue.Domain.Entities;

public class SyncQueue : BaseEntity
{
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Operation { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public SyncStatus Status { get; private set; } = SyncStatus.Pending;
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; } = 3;
    public string? LastError { get; private set; }
    public DateTime? SyncedAt { get; private set; }

    protected SyncQueue() { }

    public SyncQueue(string entityType, string entityId, string operation, string payload)
    {
        EntityType = string.IsNullOrEmpty(entityType) ? throw new ArgumentNullException(nameof(entityType)) : entityType;
        EntityId = string.IsNullOrEmpty(entityId) ? throw new ArgumentNullException(nameof(entityId)) : entityId;
        Operation = string.IsNullOrEmpty(operation) ? throw new ArgumentNullException(nameof(operation)) : operation;
        Payload = string.IsNullOrEmpty(payload) ? throw new ArgumentNullException(nameof(payload)) : payload;
    }

    public void MarkAsSynced()
    {
        Status = SyncStatus.Synced;
        SyncedAt = DateTime.UtcNow;
    }

    public void RecordFailure(string errorMessage)
    {
        LastError = errorMessage;
        RetryCount++;
        Status = RetryCount >= MaxRetries ? SyncStatus.Failed : SyncStatus.Pending;
    }
}