// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface ISecurityAnalyzer
{
    Task<Result<AnalyzerResultDTO>> AnalyzeAsync(CreateAnalyzeRequest request, CancellationToken ct);
    Task<Result<AnalyzerResultDTO>> GetReportAsync(int id, CancellationToken ct);
    Task<Result<PagedResult<AnalyzerResultDTO>>> ListReportsAsync(AnalyzerFilterDTO filter, CancellationToken ct);
    Task<Result<List<AnalyzerResultDTO>>> BatchAnalyzeAsync(BatchAnalyzeRequest request, CancellationToken ct);
    Task<Result<bool>> DeleteReportAsync(int id, CancellationToken ct);
    Task<Result<bool>> RestoreReportAsync(int id, CancellationToken ct);
    Task<Result<AnalyzerStatsDTO>> GetStatsAsync(CancellationToken ct);
    
    Task<AnalysisResult> AnalyzeAsync(string target, CancellationToken ct = default);
}