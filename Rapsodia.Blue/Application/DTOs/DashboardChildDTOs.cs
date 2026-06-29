// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.ComponentModel.DataAnnotations;

namespace Rapsodia.Blue.Application.DTOs;

public class ThreatFeedDTO
{
    [Required]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})$", ErrorMessage = "Invalid ISO 8601 format.")]
    public string Timestamp { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^(?i)(Low|Medium|High|Critical)$", ErrorMessage = "Invalid severity level.")]
    public string Severity { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Event { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;
}

public class AttackSignatureDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Percentage { get; set; }

    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^\d+$", ErrorMessage = "Count must be a numeric string.")]
    public string Count { get; set; } = string.Empty;
}

public class AssetExposureDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.0, 10.0)]
    public double Score { get; set; }
}

public class ComplianceDTO
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Percentage { get; set; }
}

public class IncidentLogDTO
{
    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Invalid characters in Identifier.")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phase { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Time { get; set; } = string.Empty;
}

public class AgentDTO
{
    [Required]
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}