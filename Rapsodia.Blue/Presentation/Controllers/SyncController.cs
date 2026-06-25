// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.AspNetCore.Mvc;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Application.Services;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncRepositoryPort _syncRepository;
    private readonly SyncOrchestratorService _orchestrator;

    public SyncController(ISyncRepositoryPort syncRepository, SyncOrchestratorService orchestrator)
    {
        _syncRepository = syncRepository;
        _orchestrator = orchestrator;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var pending = await _syncRepository.CountPendingAsync(ct);
        
        return Ok(new
        {
            offline = DatabaseToggle.UseSqlite,
            pendingSyncs = pending,
            message = pending > 0 ? $"{pending} itens aguardando sincronizacao" : "Fila vazia"
        });
    }

    [HttpPost("force")]
    public async Task<IActionResult> ForceSync(CancellationToken ct)
    {
        if (DatabaseToggle.UseSqlite)
            return BadRequest(new { message = "Sincronizacao indisponivel em modo offline. Reconecte ao Oracle primeiro." });

        await _orchestrator.SyncAsync(ct);
        var pending = await _syncRepository.CountPendingAsync(ct);
        
        return Ok(new
        {
            message = "Sincronizacao concluida",
            remainingPending = pending
        });
    }
}