// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Infrastructure.Data;
using Rapsodia.Blue.Domain.Enums;

namespace Rapsodia.Blue.Infrastructure.Repository;

public class SyncRepository : ISyncRepositoryPort
{
    private readonly BlueDbContext _context;

    public SyncRepository(BlueDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SyncQueue item, CancellationToken ct = default)
    {
        _context.Set<SyncQueue>().Add(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<SyncQueue>> GetPendingAsync(int batchSize = 50, CancellationToken ct = default)
    {
        return await _context.Set<SyncQueue>()
            .Where(s => s.Status == SyncStatus.Pending)
            .OrderBy(s => s.CreatedAt)
            .Take(batchSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(SyncQueue item, CancellationToken ct = default)
    {
        _context.Set<SyncQueue>().Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountPendingAsync(CancellationToken ct = default)
    {
        return await _context.Set<SyncQueue>()
            .CountAsync(s => s.Status == SyncStatus.Pending, ct);
    }
}