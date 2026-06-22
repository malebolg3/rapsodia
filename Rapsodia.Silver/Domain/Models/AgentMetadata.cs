// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Domain.Models;

public class AgentMetadata : BaseEntity
{
    public string AgentId { get; private set; } = string.Empty;
    public string AgentType { get; private set; } = string.Empty;
    public string Expertise { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private AgentMetadata() { }

    public AgentMetadata(string agentId, string agentType, string expertise, string ownerId)
    {
        AgentId = agentId;
        AgentType = agentType;
        Expertise = expertise;
        OwnerId = ownerId;
    }

    public void MarkAsDeleted()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = false;
    }

    public void MarkAsRestored()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }
}