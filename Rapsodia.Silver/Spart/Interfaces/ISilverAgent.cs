// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Spart.Interfaces;

public interface ISilverAgent : IGrainWithIntegerKey
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

public class AgentInfo
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}