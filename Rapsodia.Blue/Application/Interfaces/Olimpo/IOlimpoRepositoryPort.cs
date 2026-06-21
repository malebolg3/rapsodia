// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Domain.Entities.Olimpo;

namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface IOlimpoRepositoryPort
{
    Task<OlimpoCredential?> GetCredentialByKeyAsync(Guid userId, string key, string environment, CancellationToken ct);
    Task<List<OlimpoCredential>> GetAllCredentialsAsync(Guid userId, CancellationToken ct);
    Task<OlimpoCredential?> GetCredentialByIdAsync(Guid userId, Guid id, CancellationToken ct);
    Task AddCredentialAsync(OlimpoCredential credential, CancellationToken ct);
    Task UpdateCredentialAsync(OlimpoCredential credential, CancellationToken ct);
    Task DeleteCredentialAsync(OlimpoCredential credential, CancellationToken ct);
    Task<OlimpoTotpAccount?> GetTotpAccountByIdAsync(Guid userId, Guid id, CancellationToken ct);
    Task<List<OlimpoTotpAccount>> GetAllTotpAccountsAsync(Guid userId, CancellationToken ct);
    Task AddTotpAccountAsync(OlimpoTotpAccount account, CancellationToken ct);
    Task DeleteTotpAccountAsync(OlimpoTotpAccount account, CancellationToken ct);
    Task<List<OlimpoDocument>> GetAllDocumentsAsync(Guid userId, CancellationToken ct);
    Task<OlimpoDocument?> GetDocumentByIdAsync(Guid userId, Guid id, CancellationToken ct);
    Task AddDocumentAsync(OlimpoDocument document, CancellationToken ct);
    Task DeleteDocumentAsync(OlimpoDocument document, CancellationToken ct);
}