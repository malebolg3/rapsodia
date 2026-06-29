// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.Collections.Generic;

namespace Rapsodia.Blue.Domain.Entities;

public class Incident : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Open";
    public int? AssetId { get; private set; }
    public Asset? Asset { get; private set; }
    public int? AssignedToId { get; private set; }
    public string? AssignedToName { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public ICollection<IncidentComment> Comments { get; private set; } = [];

    public Incident() { }

    public void Update(string? title = null, string? description = null, string? severity = null, string? status = null, int? assetId = null)
    {
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (severity != null) Severity = severity;
        if (status != null)
        {
            Status = status;
            if (string.Equals(status, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase))
                ResolvedAt = DateTime.UtcNow;
        }
        if (assetId.HasValue) AssetId = assetId;
        MarkAsUpdated();
    }

    public void Assign(int? userId, string? userName)
    {
        AssignedToId = userId;
        AssignedToName = userName;
        MarkAsUpdated();
    }
}

public class IncidentComment : BaseEntity
{
    public int IncidentId { get; private set; }
    public Incident Incident { get; private set; } = null!;
    public string Content { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;

    public IncidentComment() { }

    public static IncidentComment Create(int incidentId, string content, string authorName)
    {
        return new IncidentComment
        {
            IncidentId = incidentId,
            Content = content,
            AuthorName = authorName,
        };
    }
}