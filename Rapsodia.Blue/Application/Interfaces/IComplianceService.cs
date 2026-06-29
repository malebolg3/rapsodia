// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IComplianceService
{
    Task<Result<ComplianceStatusDTO>> GetStatusAsync(CancellationToken ct = default);
}