// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Domain.Interfaces;

namespace Rapsodia.Silver.Infrastructure.Services;

public class ObsidianService : IObsidianService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ObsidianService> _logger;
    private readonly bool _mock;
    private readonly string? _offlinePath;
    private readonly string? _docUrl;
    private readonly string? _docKey;
    private readonly Dictionary<string, List<string>> _links = new();
    private static readonly Regex SafePathRegex = new(@"^[a-zA-Z0-9_\-]+$", RegexOptions.Compiled);

    public ObsidianService(HttpClient http, IConfiguration cfg, ILogger<ObsidianService> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
        _mock = cfg["DOC_MOCK"] == "true";
        _offlinePath = cfg["DOC_OFF"];
        _docUrl = cfg["DOC_URL"];
        _docKey = cfg["DOC_KEY"];
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

        var savedToObsidian = false;

        if (!string.IsNullOrEmpty(_docUrl))
        {
            try
            {
                await SaveNoteToObsidian(vault, noteId, markdown);
                savedToObsidian = true;
                _logger.LogInformation("Obsidian: Nota {NoteId} salva via HTTP", noteId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Obsidian indisponível. Usando fallback offline");
            }
        }

        if (!savedToObsidian && !string.IsNullOrEmpty(_offlinePath))
        {
            await SaveNoteOffline(vault, noteId, markdown);
            _logger.LogInformation("Offline: Nota {NoteId} salva localmente", noteId);
        }

        if (!savedToObsidian && string.IsNullOrEmpty(_offlinePath))
        {
            _logger.LogError("Nenhum destino disponível para salvar nota {NoteId}", noteId);
        }

        if (savedToObsidian && !string.IsNullOrEmpty(_offlinePath))
        {
            try
            {
                await SaveNoteOffline(vault, noteId, markdown);
                _logger.LogDebug("Offline: Espelho atualizado para nota {NoteId}", noteId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Offline: Falha ao atualizar espelho da nota {NoteId}", noteId);
            }
        }

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

        if (!string.IsNullOrEmpty(_docUrl))
        {
            try
            {
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_docKey}");
                var response = await _http.GetAsync($"{_docUrl}/vault/{vault}/note/{noteId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                _logger.LogWarning("Obsidian indisponível para leitura. Tentando offline");
            }
        }

        if (!string.IsNullOrEmpty(_offlinePath))
        {
            var filePath = Path.Combine(_offlinePath, vault, $"{noteId}.md");
            if (File.Exists(filePath))
                return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }

        throw new FileNotFoundException($"Nota não encontrada: {noteId}");
    }

    public async Task<List<string>> SearchNotesAsync(string vault, string query)
    {
        ValidateInput(vault);

        if (_mock) return new List<string> { "S20240606143000", "R20240606143100", "B20240606143200", "V20240606150000" };

        var results = new HashSet<string>();

        if (!string.IsNullOrEmpty(_docUrl))
        {
            try
            {
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_docKey}");
                var response = await _http.GetAsync($"{_docUrl}/vault/{vault}/search?q={Uri.EscapeDataString(query)}");
                response.EnsureSuccessStatusCode();
                var searchResults = await response.Content.ReadFromJsonAsync<List<ObsidianSearchResult>>();
                if (searchResults != null)
                    foreach (var r in searchResults) results.Add(r.NoteId);
            }
            catch
            {
                _logger.LogWarning("Busca Obsidian falhou. Complementando com offline");
            }
        }

        if (!string.IsNullOrEmpty(_offlinePath))
        {
            var dir = Path.Combine(_offlinePath, vault);
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var content = await File.ReadAllTextAsync(file, Encoding.UTF8);
                    if (content.Contains(query, StringComparison.OrdinalIgnoreCase))
                        results.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
        }

        return results.ToList();
    }

    public async Task<bool> HealthCheckAsync()
    {
        if (_mock) return true;

        if (!string.IsNullOrEmpty(_docUrl))
        {
            try
            {
                var response = await _http.GetAsync($"{_docUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch { }
        }

        if (!string.IsNullOrEmpty(_offlinePath))
            return Directory.Exists(_offlinePath);

        return false;
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
            "scans" => "R", "exploits" => "R",
            "vulnerabilities" => "B", "incidents" => "B", "defense" => "B",
            "ai-analysis" => "S", "events" => "S", "knowledge" => "S",
            "honeypots" => "V", "soc" => "V", "cyber-ranges" => "V", "labs" => "V",
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
        title = title.ToLower().Replace(" ", "-").Replace(":", "").Replace("/", "-")
            .Replace("\\", "-").Replace(".", "").Replace(",", "").Replace("(", "")
            .Replace(")", "").Replace("[", "").Replace("]", "").Replace("--", "-").Trim('-');
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
        if (!relatedNoteIds.Any()) return "Nenhuma";
        var sb = new StringBuilder();
        foreach (var id in relatedNoteIds) sb.AppendLine($"- [[{id}]]");
        return sb.ToString().TrimEnd();
    }

    private string BuildFrontlinks(string noteId)
    {
        if (!_links.ContainsKey(noteId) || !_links[noteId].Any())
            return "Nenhuma (atualiza ao referenciar)";
        var sb = new StringBuilder();
        foreach (var id in _links[noteId]) sb.AppendLine($"- [[{id}]]");
        return sb.ToString().TrimEnd();
    }

    private async Task SaveNoteOffline(string vault, string noteId, string markdown)
    {
        var dir = Path.Combine(_offlinePath!, vault);
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, $"{noteId}.md");
        await File.WriteAllTextAsync(filePath, markdown, Encoding.UTF8);
    }

    private async Task SaveNoteToObsidian(string vault, string noteId, string markdown)
    {
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_docKey}");
        var payload = new { vault, noteId, content = markdown, timestamp = DateTime.UtcNow };
        var response = await _http.PostAsJsonAsync($"{_docUrl}/vault/{vault}/note/{noteId}", payload);
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
                    updatedContent = existingContent.Insert(
                        existingContent.LastIndexOf(backlinkSection) + backlinkSection.Length,
                        $"\n- [[{sourceNoteId}]]");
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