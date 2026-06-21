// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Red.Domain.Entities;

namespace Rapsodia.Red.Domain.Interfaces;

public interface IScanPort
{
    Task<bool> RunScanAsync(ScanTarget target);
}