// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Violet.Application.DTOs;

public class CreateLabRequest
{
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = "kalilinux/kali-rolling:latest";
    public string Network { get; set; } = "isolated";
    public List<string>? Ports { get; set; }
    public Dictionary<string, string>? EnvVars { get; set; }
    public int TtlMinutes { get; set; } = 60;
}

public class LabResultDTO
{
    public string ContainerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public List<string> Ports { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class LabSnapshotDTO
{
    public string SnapshotId { get; set; } = string.Empty;
    public string ContainerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
