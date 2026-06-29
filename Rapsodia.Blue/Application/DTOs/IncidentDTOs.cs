// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rapsodia.Blue.Application.DTOs;

public class CreateIncidentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^(?i)(Low|Medium|High|Critical)$", ErrorMessage = "Invalid severity level.")]
    public string Severity { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? AssetId { get; set; }

    public List<int>? VulnIds { get; set; }
}

public class EditIncidentRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"^(?i)(Low|Medium|High|Critical)$", ErrorMessage = "Invalid severity level.")]
    public string? Severity { get; set; }
}

public class IncidentFilterDTO
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    [MaxLength(20)]
    public string? Severity { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [Range(1, int.MaxValue)]
    public int? AssetId { get; set; }
}

public class IncidentResultDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssetId { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public List<IncidentCommentDTO> Comments { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class IncidentCommentDTO
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AssignIncidentRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}

public class ChangeStatusRequest
{
    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^(?i)(Open|InProgress|Resolved|Closed)$", ErrorMessage = "Invalid status value.")]
    public string Status { get; set; } = string.Empty;
}

public class AddCommentRequest
{
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

public class IncidentStatsDTO
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int InProgressIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int ActiveCount { get; set; }
    public int AverageMttr { get; set; }
    public Dictionary<string, int> BySeverity { get; set; } = [];
    public List<IncidentLogDTO> RecentLogs { get; set; } = [];
}