// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Domain.Interfaces;

public interface IObsidianService
{
    Task<string> QueryAsync(string query);
    Task<bool> SyncDocumentAsync(string path, string content);
    Task<string> GetDocumentAsync(string path);
}