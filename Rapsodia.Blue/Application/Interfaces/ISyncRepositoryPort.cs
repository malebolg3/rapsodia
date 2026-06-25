// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Interfaces;

public interface ISyncRepositoryPort
{
    Task AddAsync(SyncQueue item, CancellationToken ct = default);
    Task<IEnumerable<SyncQueue>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default);
    Task UpdateAsync(SyncQueue item, CancellationToken ct = default);
    Task<int> CountPendingAsync(CancellationToken ct = default);
}