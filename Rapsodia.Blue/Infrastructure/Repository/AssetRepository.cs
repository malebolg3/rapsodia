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

public class AssetRepository : IAssetRepositoryPort
{
    private readonly BlueDbContext _context;

    public AssetRepository(BlueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> ListAllActiveAsync()
    {
        return await _context.Assets
            .Where(a => a.DeletedAt == null)
            .Include(a => a.AssetType)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Asset>> ListAllAsync()
    {
        return await _context.Assets
            .Include(a => a.AssetType)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.AssetType)
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task<Asset?> GetByIdWithVulnsAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.AssetType)
            .Include(a => a.AssetVulns)
                .ThenInclude(av => av.Vuln)
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _context.Assets.AnyAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task SaveAsync(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Assets.AnyAsync(a => a.Name == name && a.DeletedAt == null);
    }

    public async Task<bool> ExistsByNameExceptIdAsync(string name, int excludeId)
    {
        return await _context.Assets.AnyAsync(a => a.Name == name && a.Id != excludeId && a.DeletedAt == null);
    }
}