// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Violet.Domain;

public class LabEnvironment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = "ubuntu:latest";
    public string ContainerId { get; set; } = string.Empty;
    public LabType Type { get; set; } = LabType.Sandbox;
    public LabStatus Status { get; set; } = LabStatus.Creating;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DestroyedAt { get; set; }
}

public enum LabType
{
    Sandbox,
    Honeypot,
    Defense,
    Training,
    Custom
}

public enum LabStatus
{
    Creating,
    Running,
    Stopped,
    Failed,
    Destroyed
}
