// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Domain.Interfaces;

public interface IObsidianService
{
    Task<string> AppendNoteAsync(string vault, string content);
    Task<string> AppendNoteWithLinksAsync(string vault, string content, IEnumerable<string>? relatedNoteIds = null, IEnumerable<string>? tags = null);
    Task<string> ReadNoteAsync(string vault, string noteId);
    Task<List<string>> SearchNotesAsync(string vault, string query);
    Task<bool> HealthCheckAsync();
}