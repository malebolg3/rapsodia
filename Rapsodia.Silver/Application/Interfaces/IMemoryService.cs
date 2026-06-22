// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Application.Interfaces;

public interface IMemoryService
{
    Task SaveMemory(string agent, string prompt, string response);
    Task<string> GenerateMarkdown(string agent, string prompt, string response);
    Task<List<string>> SearchMemory(string query, string agent);
}