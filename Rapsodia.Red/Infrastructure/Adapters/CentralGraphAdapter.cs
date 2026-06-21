// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Red.Domain.Interfaces;

namespace Rapsodia.Red.Infrastructure.Adapters;

public class CentralGraphAdapter : IGraphPort
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<CentralGraphAdapter> _logger;

    public CentralGraphAdapter(HttpClient http, IConfiguration cfg, ILogger<CentralGraphAdapter> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task ConnectAsync(Guid sourceId, string originType, Guid targetId, string targetType, string relationType)
    {
        var bluePort = _cfg["PORT_BLU"] ?? "5073";
        var payload = new { sourceId, originType, targetId, targetType, relationType, timestamp = DateTime.UtcNow };
        try
        {
            await _http.PostAsJsonAsync($"http://localhost:{bluePort}/api/graph/relation", payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Graph connection failed");
        }
    }
}
