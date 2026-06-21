// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Application.DTOs;

public record AnalysisResult(string TargetId, List<string> Findings);

public class CreateAnalyzeRequest
{
    public int AssetId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class AnalyzerFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? AnalysisType { get; set; }
    public int? AssetId { get; set; }
}

public class BatchAnalyzeRequest
{
    public List<int> AssetIds { get; set; } = new();
    public string AnalysisType { get; set; } = string.Empty;
}

public class AnalyzerResultDTO
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AnalyzerStatsDTO
{
    public int TotalAnalyses { get; set; }
    public int CompletedAnalyses { get; set; }
    public int FailedAnalyses { get; set; }
}