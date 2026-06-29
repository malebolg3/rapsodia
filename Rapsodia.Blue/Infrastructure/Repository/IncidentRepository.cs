// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Infrastructure.Repository;

public class IncidentRepository : IIncidentRepository
{
    private readonly BlueDbContext _db;

    public IncidentRepository(BlueDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Incident?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Incidents
            .Include(i => i.Comments)
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null, ct);

    public async Task<List<Incident>> ListAllAsync(CancellationToken ct = default)
        => await _db.Incidents
            .Include(i => i.Comments)
            .Where(i => i.DeletedAt == null)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

    public async Task<Incident> SaveAsync(Incident incident, CancellationToken ct = default)
    {
        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<Incident> UpdateAsync(Incident incident, CancellationToken ct = default)
    {
        _db.Incidents.Update(incident);
        await _db.SaveChangesAsync(ct);
        return incident;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var incident = await _db.Incidents.FindAsync(new object[] { id }, ct);
        if (incident == null) return false;
        incident.MarkAsDeleted();
        await _db.SaveChangesAsync(ct);
        return true;
    }
}