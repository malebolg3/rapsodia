// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Infrastructure.Repository;

public class VulnRepository : IVulnRepositoryPort
{
    private readonly BlueDbContext _context;

    public VulnRepository(BlueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vuln>> ListAllAsync()
    {
        return await _context.Vulns
            .Where(v => v.DeletedAt == null)
            .Include(v => v.ParentVuln)
            .Include(v => v.ChildVulns.Where(c => c.DeletedAt == null))
            .Include(v => v.RelatedVulns)
            .Include(v => v.AssetVulns).ThenInclude(av => av.Asset)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Vuln?> GetByIdAsync(int id)
    {
        return await _context.Vulns
            .FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null);
    }

    public async Task<Vuln?> GetByIdWithRelationsAsync(int id)
    {
        return await _context.Vulns
            .Include(v => v.ParentVuln)
            .Include(v => v.ChildVulns.Where(c => c.DeletedAt == null))
            .Include(v => v.RelatedVulns)
            .Include(v => v.AssetVulns).ThenInclude(av => av.Asset)
            .FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null);
    }

    public async Task<IEnumerable<Vuln>> GetByIdsAsync(List<int> ids)
    {
        return await _context.Vulns
            .Where(v => ids.Contains(v.Id) && v.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.Vulns.AnyAsync(v => v.Id == id && v.DeletedAt == null);
    }

    public async Task SaveAsync(Vuln vuln)
    {
        _context.Vulns.Add(vuln);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vuln vuln)
    {
        _context.Vulns.Update(vuln);
        await _context.SaveChangesAsync();
    }
}