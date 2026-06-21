// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Application.Interfaces;

namespace Rapsodia.Silver.Application.Services;

public class ObsidianService : IObsidianService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ObsidianService> _logger;
    private readonly bool _mock;
    private readonly Dictionary<string, List<string>> _links = new();
    private static readonly Regex SafePathRegex = new(@"^[a-zA-Z0-9_\-]+$", RegexOptions.Compiled);

    public ObsidianService(HttpClient http, IConfiguration cfg, ILogger<ObsidianService> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
        _mock = cfg["DOC_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DOC_URL"]);
    }

    public async Task<string> AppendNoteAsync(string vault, string content)
    {
        return await AppendNoteWithLinksAsync(vault, content, null, null);
    }

    public async Task<string> AppendNoteWithLinksAsync(string vault, string content, IEnumerable<string>? relatedNoteIds = null, IEnumerable<string>? tags = null)
    {
        ValidateInput(vault);
        var noteId = GenerateNoteId(vault);
        var title = GenerateTitle(content);
        var tagList = BuildTagList(vault, tags);
        var relatedList = relatedNoteIds?.ToList() ?? new List<string>();
        var backlinks = BuildBacklinks(relatedList);
        var frontlinks = BuildFrontlinks(noteId);

        foreach (var relatedId in relatedList)
        {
            ValidateInput(relatedId);
            if (_links.ContainsKey(relatedId))
                _links[relatedId].Add(noteId);
            else
                _links[relatedId] = new List<string> { noteId };
        }

        var markdown = $"""
---
id: {noteId}
vault: {vault}
title: {title}
created: {DateTime.UtcNow:O}
tags: [{tagList}]
related: [{string.Join(", ", relatedList)}]
---

# {title}

{tagList.Replace(",", " ")}

{content}

## 🔗 Related Notes
{backlinks}
## 📌 Referenced By
{frontlinks}
""";

        if (_mock)
        {
            _logger.LogInformation("Mock: Nota {NoteId} ({Title}) → vault {Vault}", noteId, title, vault);
            return noteId;
        }

        await SaveNoteToObsidian(vault, noteId, markdown);

        foreach (var relatedId in relatedList)
        {
            await AppendBacklinkToNote(relatedId, noteId);
        }

        return noteId;
    }

    public async Task<string> ReadNoteAsync(string vault, string noteId)
    {
        ValidateInput(vault);
        ValidateInput(noteId);

        if (_mock) return $"# Mock Note {noteId}\n\nSecurity analysis result placeholder.";

        var docUrl = _cfg["DOC_URL"]!;
        var apiKey = _cfg["DOC_KEY"];

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await _http.GetAsync($"{docUrl}/vault/{vault}/note/{noteId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<List<string>> SearchNotesAsync(string vault, string query)
    {
        ValidateInput(vault);

        if (_mock) return new List<string> { "S20240606143000", "R20240606143100", "B20240606143200", "V20240606150000" };

        var docUrl = _cfg["DOC_URL"]!;
        var apiKey = _cfg["DOC_KEY"];

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await _http.GetAsync($"{docUrl}/vault/{vault}/search?q={Uri.EscapeDataString(query)}");
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<ObsidianSearchResult>>();
        return results?.Select(r => r.NoteId).ToList() ?? new List<string>();
    }

    public async Task<bool> HealthCheckAsync()
    {
        if (_mock) return true;

        try
        {
            var docUrl = _cfg["DOC_URL"]!;
            var response = await _http.GetAsync($"{docUrl}/health");
            return response.IsSuccessStatusCode;
            }
        catch
        {
            return false;
        }
    }

    private void ValidateInput(string input)
    {
        if (string.IsNullOrEmpty(input) || !SafePathRegex.IsMatch(input))
            throw new ArgumentException("Input inválido detectado.");
    }

    private string GenerateNoteId(string vault)
    {
        var prefix = vault switch
        {
            "scans" => "R",
            "exploits" => "R",
            "vulnerabilities" => "B",
            "incidents" => "B",
            "defense" => "B",
            "ai-analysis" => "S",
            "events" => "S",
            "knowledge" => "S",
            "honeypots" => "V",
            "soc" => "V",
            "cyber-ranges" => "V",
            "labs" => "V",
            _ => "X"
        };

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{prefix}{timestamp}";
    }

    private static string GenerateTitle(string content)
    {
        var firstLine = content.Split('\n')[0].Trim();

        if (firstLine.StartsWith("## ") || firstLine.StartsWith("### "))
            firstLine = firstLine[3..].Trim();
        if (firstLine.StartsWith("# "))
            firstLine = firstLine[2..].Trim();

        var title = firstLine.Length > 50 ? firstLine[..50] : firstLine;

        title = title.ToLower()
            .Replace(" ", "-")
            .Replace(":", "")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("--", "-")
            .Trim('-');

        return title.Length > 60 ? title[..60] : title;
    }

    private static string BuildTagList(string vault, IEnumerable<string>? tags)
    {
        var allTags = new List<string> { vault };
        if (tags != null) allTags.AddRange(tags);
        return string.Join(", ", allTags.Distinct());
    }

    private static string BuildBacklinks(List<string> relatedNoteIds)
    {
        if (!relatedNoteIds.Any())
            return "Nenhuma";

        var sb = new StringBuilder();
        foreach (var id in relatedNoteIds)
        {
            sb.AppendLine($"- [[{id}]]");
        }
        return sb.ToString().TrimEnd();
    }

    private string BuildFrontlinks(string noteId)
    {
        if (!_links.ContainsKey(noteId) || !_links[noteId].Any())
            return "Nenhuma (atualiza ao referenciar)";

        var sb = new StringBuilder();
        foreach (var id in _links[noteId])
        {
            sb.AppendLine($"- [[{id}]]");
        }
        return sb.ToString().TrimEnd();
    }

    private async Task SaveNoteToObsidian(string vault, string noteId, string markdown)
    {
        var docUrl = _cfg["DOC_URL"]!;
        var apiKey = _cfg["DOC_KEY"];

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var payload = new { vault, noteId, content = markdown, timestamp = DateTime.UtcNow };
        var response = await _http.PostAsJsonAsync($"{docUrl}/vault/{vault}/note/{noteId}", payload);
        response.EnsureSuccessStatusCode();
    }

    private async Task AppendBacklinkToNote(string targetNoteId, string sourceNoteId)
    {
        try
        {
            var existingContent = await ReadNoteAsync("any", targetNoteId);
            var backlinkSection = "## 📌 Referenced By";

            if (existingContent.Contains(backlinkSection))
            {
                var updatedContent = existingContent.Replace(
                    "Nenhuma (atualiza ao referenciar)",
                    $"Nenhuma (atualiza ao referenciar)\n- [[{sourceNoteId}]]");

                if (updatedContent == existingContent)
                {
                    updatedContent = existingContent.Insert(
                        existingContent.LastIndexOf(backlinkSection) + backlinkSection.Length,
                        $"\n- [[{sourceNoteId}]]");
                }

                await SaveNoteToObsidian("any", targetNoteId, updatedContent);
            }
        }
        catch
        {
            _logger.LogDebug("Não foi possível atualizar backlink de {TargetNote}", targetNoteId);
        }
    }
}

public class ObsidianResponse { public string NoteId { get; set; } = string.Empty; }
public class ObsidianSearchResult { public string NoteId { get; set; } = string.Empty; public string Title { get; set; } = string.Empty; }