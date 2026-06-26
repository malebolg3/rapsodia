// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Orleans;

namespace Rapsodia.Silver.Spart.Interfaces;

public interface ISilverAgent : IGrainWithGuidKey
{
    Task<string> AnalyzeAsync(string input);
    Task<string> GetStatusAsync();
    Task SetContextAsync(string context);
    Task<string> CreateAgent(string agentType, string expertise, string ownerId);
    Task<List<AgentInfo>> ListAgents();
    Task<bool> DeactivateAgent(string agentId);
    Task<bool> ActivateAgent(string agentId);
    Task<List<string>> SearchMemory(string query);
}

[GenerateSerializer]
public sealed class AgentInfo
{
    [Id(0)]
    public string AgentId { get; init; } = string.Empty;

    [Id(1)]
    public string AgentType { get; init; } = string.Empty;

    [Id(2)]
    public string Expertise { get; init; } = string.Empty;

    [Id(3)]
    public string OwnerId { get; init; } = string.Empty;

    [Id(4)]
    public bool IsActive { get; set; }

    [Id(5)]
    public DateTime CreatedAt { get; init; }

    [Id(6)]
    public DateTime? DeactivatedAt { get; set; }
}