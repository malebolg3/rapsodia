// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class ComplianceService : IComplianceService
{
    public Task<Result<ComplianceStatusDTO>> GetStatusAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Result<ComplianceStatusDTO>.Ok(new ComplianceStatusDTO
        {
            OverallScore = 0,
            Items = []
        }));
    }
}