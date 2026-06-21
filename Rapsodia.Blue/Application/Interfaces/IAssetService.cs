// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IAssetService
{
    Task<ResponseModel<List<AssetResponse>>> ListAsync(CancellationToken ct = default);
    Task<ResponseModel<AssetResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ResponseModel<AssetResponse>> CreateAsync(CreateAssetRequest request, CancellationToken ct = default);
    Task<ResponseModel<AssetResponse>> UpdateAsync(int id, EditAssetRequest request, CancellationToken ct = default);
    Task<ResponseModel<bool>> DisableAsync(int id, CancellationToken ct = default);
    Task<ResponseModel<bool>> EnableAsync(int id, CancellationToken ct = default);
    Task<ResponseModel<AssetResponse>> AddRelatedAsync(int assetId, int relatedAssetId, CancellationToken ct = default);
    Task<ResponseModel<AssetResponse>> RemoveRelatedAsync(int assetId, int relatedAssetId, CancellationToken ct = default);
}