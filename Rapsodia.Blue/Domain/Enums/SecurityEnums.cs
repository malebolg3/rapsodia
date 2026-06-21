// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Domain.Enums;

public enum VulnAssetStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    FalsePositive = 4
}

public enum VulnerabilityEnvironment
{
    Development = 1,
    Staging = 2,
    Production = 3
}

public enum VulnerabilityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ScanStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4
}