// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IReportService
{
    Task<Result<ReportResultDTO>> GenerateAsync(CreateReportRequest request, CancellationToken ct);
    Task<Result<ReportResultDTO>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<PagedResult<ReportResultDTO>>> ListAsync(ReportFilterDTO filter, CancellationToken ct);
    Task<Result<ReportFileDTO>> ExportPdfAsync(int id, CancellationToken ct);
    Task<Result<ReportResultDTO>> ExportJsonAsync(int id, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);
    Task<Result<bool>> RestoreAsync(int id, CancellationToken ct);
    Task<Result<DashboardSummaryDTO>> GetDashboardAsync(CancellationToken ct);
}
