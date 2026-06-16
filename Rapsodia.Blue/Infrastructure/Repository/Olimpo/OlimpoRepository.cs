using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Entities.Olimpo;
using Rapsodia.Blue.Application.Interfaces.Olimpo;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Infrastructure.Repository.Olimpo;

public class OlimpoRepository : IOlimpoRepositoryPort
{
    private readonly BlueDbContext _context;

    public OlimpoRepository(BlueDbContext context)
    {
        _context = context;
    }

    public async Task<OlimpoCredential?> GetCredentialByKeyAsync(Guid userId, string key, string environment, CancellationToken ct)
    {
        return await _context.OlimpoCredentials
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Key == key && c.Environment == environment, ct);
    }

    public async Task<List<OlimpoCredential>> GetAllCredentialsAsync(Guid userId, CancellationToken ct)
    {
        return await _context.OlimpoCredentials
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Environment).ThenBy(c => c.Key)
            .ToListAsync(ct);
    }

    public async Task<OlimpoCredential?> GetCredentialByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        return await _context.OlimpoCredentials
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id, ct);
    }

    public async Task AddCredentialAsync(OlimpoCredential credential, CancellationToken ct)
    {
        await _context.OlimpoCredentials.AddAsync(credential, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateCredentialAsync(OlimpoCredential credential, CancellationToken ct)
    {
        _context.OlimpoCredentials.Update(credential);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteCredentialAsync(OlimpoCredential credential, CancellationToken ct)
    {
        _context.OlimpoCredentials.Remove(credential);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<OlimpoTotpAccount?> GetTotpAccountByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        return await _context.OlimpoTotpAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Id == id, ct);
    }

    public async Task<List<OlimpoTotpAccount>> GetAllTotpAccountsAsync(Guid userId, CancellationToken ct)
    {
        return await _context.OlimpoTotpAccounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Issuer).ThenBy(a => a.Label)
            .ToListAsync(ct);
    }

    public async Task AddTotpAccountAsync(OlimpoTotpAccount account, CancellationToken ct)
    {
        await _context.OlimpoTotpAccounts.AddAsync(account, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteTotpAccountAsync(OlimpoTotpAccount account, CancellationToken ct)
    {
        _context.OlimpoTotpAccounts.Remove(account);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<OlimpoDocument>> GetAllDocumentsAsync(Guid userId, CancellationToken ct)
    {
        return await _context.OlimpoDocuments
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<OlimpoDocument?> GetDocumentByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        return await _context.OlimpoDocuments
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == id, ct);
    }

    public async Task AddDocumentAsync(OlimpoDocument document, CancellationToken ct)
    {
        await _context.OlimpoDocuments.AddAsync(document, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteDocumentAsync(OlimpoDocument document, CancellationToken ct)
    {
        _context.OlimpoDocuments.Remove(document);
        await _context.SaveChangesAsync(ct);
    }
}