// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Domain.Models;

public class SecurityGraph
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public User? User { get; protected set; }
    public ICollection<GraphEdge> Edges { get; protected set; } = new List<GraphEdge>();
}

public class GraphEdge
{
    public Guid SourceId { get; init; }
    public Guid TargetId { get; init; }
    public string OriginType { get; init; } = null!;
    public string TargetType { get; init; } = null!;
    public string RelationType { get; init; } = null!;
    public float? Weight { get; set; }
    public string? Embedding { get; set; }
}

public class GraphHistory
{
    public int Id { get; init; }
    public string OriginType { get; init; } = string.Empty;
    public Guid SourceId { get; init; }
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
}

public class EntropyHistory : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid ConversationId { get; init; }
    public double ShannonEntropy { get; protected set; }
    public double RiskScore { get; protected set; }
    public int AssetCount { get; protected set; }
    public int VulnCount { get; protected set; }
    public int RelationshipCount { get; protected set; }
    public string AnalysisSnapshot { get; protected set; } = string.Empty;
    public string RiskNarrative { get; protected set; } = string.Empty;
}

public class ConversationHistory : BaseEntity
{
    public Guid UserId { get; init; }
    public Guid ConversationId { get; init; }
    public string UserMessage { get; init; } = string.Empty;
    public string AssistantResponse { get; init; } = string.Empty;
    public double ContextEntropy { get; init; }
    public string Metadata { get; init; } = string.Empty;
}