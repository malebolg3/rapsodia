// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Application.DTOs;

public class CreateIncidentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int? AssetId { get; set; }
    public List<int>? VulnIds { get; set; }
}

public class EditIncidentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
}

public class IncidentFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Severity { get; set; }
    public string? Status { get; set; }
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
    public List<IncidentCommentDTO> Comments { get; set; } = new();
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
    public int UserId { get; set; }
}

public class ChangeStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class IncidentStatsDTO
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int InProgressIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public Dictionary<string, int> BySeverity { get; set; } = new();
}