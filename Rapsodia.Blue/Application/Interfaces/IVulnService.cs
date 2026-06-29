// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IVulnService
{
    Task<ResponseModel<List<VulnResponse>>> ListAsync(CancellationToken ct = default);
    Task<ResponseModel<VulnResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ResponseModel<VulnResponse>> CreateAsync(CreateVulnRequest request, CancellationToken ct = default);
    Task<ResponseModel<VulnResponse>> UpdateAsync(int id, EditVulnRequest request, CancellationToken ct = default);
    Task<ResponseModel<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<ResponseModel<bool>> RestoreAsync(int id, CancellationToken ct = default);
    Task<ResponseModel<VulnResponse>> AddToAssetAsync(int assetId, AddVulnToAssetRequest request, CancellationToken ct = default);
    Task<Result<VulnStatsDTO>> GetStatsAsync(CancellationToken ct = default);
    Task<Result<VulnTrendDTO>> GetTrendAsync(TrendFilterDTO filter, CancellationToken ct = default);
}