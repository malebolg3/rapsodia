// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Red.Application.DTOs;
using Rapsodia.Red.Domain.Common;

namespace Rapsodia.Red.Application.Interfaces;

public interface IScanService
{
    Task<Result<ScanResultDTO>> StartScanAsync(ScanRequest request, CancellationToken ct);
    Task<Result<ScanResultDTO>> GetScanStatusAsync(Guid scanId, CancellationToken ct);
    Task<Result<PagedResult<ScanResultDTO>>> ListScansAsync(ScanFilterDTO filter, CancellationToken ct);
    Task<Result<ScanResultDTO>> StopScanAsync(Guid scanId, CancellationToken ct);
    Task<Result<List<ScanFindingDTO>>> GetFindingsAsync(Guid scanId, CancellationToken ct);
}